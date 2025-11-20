using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Domain.Entities;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Services;
using CRM.Shared.Config;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CRM.Application.Quotations.Services
{
    public class QuotationPdfGenerationService : IQuotationPdfGenerationService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<QuotationPdfGenerationService> _logger;
        private readonly QuotationManagementSettings _settings;
        private readonly ICompanyDetailsService _companyDetailsService;
        private readonly IConfiguration _configuration;

        public QuotationPdfGenerationService(
            IMemoryCache cache,
            ILogger<QuotationPdfGenerationService> logger,
            IOptions<QuotationManagementSettings> settings,
            ICompanyDetailsService companyDetailsService,
            IConfiguration configuration)
        {
            _cache = cache;
            _logger = logger;
            _settings = settings.Value;
            _companyDetailsService = companyDetailsService;
            _configuration = configuration;
        }

        public async Task<byte[]> GenerateQuotationPdfAsync(Quotation quotation)
        {
            // Check cache first
            var cacheKey = BuildCacheKey(quotation);
            if (_cache.TryGetValue(cacheKey, out byte[]? cachedPdf) && cachedPdf != null)
            {
                _logger.LogInformation("Returning cached PDF for quotation {QuotationId}", quotation.QuotationId);
                return cachedPdf;
            }

            _logger.LogInformation("Generating PDF for quotation {QuotationId}", quotation.QuotationId);

            QuestPDF.Settings.License = LicenseType.Community;

            // Get company details (from snapshot or service)
            CompanyDetailsDto? companyDetails = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(quotation.CompanyDetailsSnapshot))
                {
                    _logger.LogInformation("Loading company details from snapshot for quotation {QuotationId}", quotation.QuotationId);
                    companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(quotation.CompanyDetailsSnapshot);
                    if (companyDetails != null)
                    {
                        _logger.LogInformation("Successfully loaded company details from snapshot. Company: {CompanyName}", companyDetails.CompanyName);
                    }
                }
                else
                {
                    _logger.LogWarning("No company details snapshot found for quotation {QuotationId}, loading from service", quotation.QuotationId);
                    // Fallback to current company details if snapshot is not available
                    companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                    if (companyDetails != null)
                    {
                        _logger.LogInformation("Loaded company details from service. Company: {CompanyName}", companyDetails.CompanyName);
                    }
                    else
                    {
                        _logger.LogWarning("No company details found in service for quotation {QuotationId}", quotation.QuotationId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load company details for quotation {QuotationId}. Error: {Error}", quotation.QuotationId, ex.Message);
                // Try to load from service as last resort
                try
                {
                    companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                    _logger.LogInformation("Loaded company details from service as fallback for quotation {QuotationId}", quotation.QuotationId);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Failed to load company details from service as fallback for quotation {QuotationId}", quotation.QuotationId);
                }
            }

            // Capture company details in local variable for use in Document.Create
            var companyDetailsForPdf = companyDetails;
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => Header(c, quotation, companyDetailsForPdf));
                    page.Content().Element(c => Content(c, quotation, companyDetailsForPdf));
                    page.Footer().Element(c => Footer(c, quotation, companyDetailsForPdf));
                });
            }).GeneratePdf();

            var cacheDuration = GetCacheDuration();
            if (cacheDuration.HasValue)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = cacheDuration
                };
                _cache.Set(cacheKey, pdfBytes, cacheOptions);
            }

            return pdfBytes;
        }

        private void Header(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.Row(row =>
            {
                // Company Logo and Info (Left) - 60% width
                row.RelativeItem(3).Column(column =>
                {
                    // Logo or Company Name
                    if (companyDetails != null && !string.IsNullOrWhiteSpace(companyDetails.LogoUrl))
                    {
                        try
                        {
                            // Convert relative path to absolute file path
                            string imagePath = companyDetails.LogoUrl;
                            if (!Path.IsPathRooted(imagePath) && !imagePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                // Get base path from configuration or use default
                                var configuredPath = _configuration["FileStorage:BasePath"];
                                var basePath = string.IsNullOrWhiteSpace(configuredPath)
                                    ? Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads")
                                    : configuredPath;
                                
                                // Remove leading slash if present
                                var cleanPath = imagePath.TrimStart('/');
                                
                                // The logoUrl format is: "/uploads/company-logos/filename"
                                // We need to extract just "company-logos/filename"
                                if (cleanPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                                {
                                    cleanPath = cleanPath.Substring("uploads/".Length);
                                }
                                
                                // Build absolute path - basePath might be "wwwroot/uploads" or just the configured path
                                // If basePath already ends with "uploads", we don't need to add it again
                                var fullPath = Path.Combine(basePath, cleanPath);
                                
                                // Normalize the path
                                fullPath = Path.GetFullPath(fullPath);
                                
                                _logger.LogInformation("Attempting to load logo from path: {Path}", fullPath);
                                
                                if (File.Exists(fullPath))
                                {
                                    imagePath = fullPath;
                                    _logger.LogInformation("Logo file found at: {Path}", fullPath);
                                }
                                else
                                {
                                    // Try alternative path resolution
                                    // If basePath is just the configured path without "uploads", try adding it
                                    var alternativePath = Path.Combine(basePath, "uploads", cleanPath);
                                    alternativePath = Path.GetFullPath(alternativePath);
                                    
                                    if (File.Exists(alternativePath))
                                    {
                                        imagePath = alternativePath;
                                        _logger.LogInformation("Logo file found at alternative path: {Path}", alternativePath);
                                    }
                                    else
                                    {
                                        // Try with wwwroot directly
                                        var wwwrootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", cleanPath);
                                        wwwrootPath = Path.GetFullPath(wwwrootPath);
                                        
                                        if (File.Exists(wwwrootPath))
                                        {
                                            imagePath = wwwrootPath;
                                            _logger.LogInformation("Logo file found at wwwroot path: {Path}", wwwrootPath);
                                        }
                                        else
                                        {
                                            _logger.LogWarning("Logo file not found. Tried paths: {Path1}, {Path2}, {Path3}", fullPath, alternativePath, wwwrootPath);
                                            throw new FileNotFoundException($"Logo file not found. Tried: {fullPath}, {alternativePath}, {wwwrootPath}");
                                        }
                                    }
                                }
                            }
                            
                            _logger.LogInformation("Loading logo image from: {ImagePath}", imagePath);
                            column.Item().Height(50, Unit.Point).Image(imagePath).FitWidth();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to load company logo from {LogoUrl}, showing company name instead. Error: {Error}", companyDetails.LogoUrl, ex.Message);
                            // If image loading fails, show company name
                            if (!string.IsNullOrWhiteSpace(companyDetails.CompanyName))
                            {
                                column.Item().Text(companyDetails.CompanyName).FontSize(16).Bold();
                            }
                        }
                    }
                    else if (companyDetails != null && !string.IsNullOrWhiteSpace(companyDetails.CompanyName))
                    {
                        column.Item().Text(companyDetails.CompanyName).FontSize(16).Bold();
                    }
                    else
                    {
                        column.Item().Text("Your Company Name").FontSize(16).Bold().FontColor(Colors.Grey.Medium);
                    }

                    // Company Address
                    if (companyDetails != null)
                    {
                        if (!string.IsNullOrWhiteSpace(companyDetails.CompanyAddress))
                        {
                            column.Item().Text(companyDetails.CompanyAddress).FontSize(9);
                        }
                        
                        var addressParts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(companyDetails.City)) addressParts.Add(companyDetails.City);
                        if (!string.IsNullOrWhiteSpace(companyDetails.State)) addressParts.Add(companyDetails.State);
                        if (!string.IsNullOrWhiteSpace(companyDetails.PostalCode)) addressParts.Add(companyDetails.PostalCode);
                        if (!string.IsNullOrWhiteSpace(companyDetails.Country)) addressParts.Add(companyDetails.Country);
                        if (addressParts.Any())
                        {
                            column.Item().Text(string.Join(", ", addressParts)).FontSize(9);
                        }

                        // Contact Info
                        var contactInfo = new List<string>();
                        if (!string.IsNullOrWhiteSpace(companyDetails.ContactEmail)) contactInfo.Add($"Email: {companyDetails.ContactEmail}");
                        if (!string.IsNullOrWhiteSpace(companyDetails.ContactPhone)) contactInfo.Add($"Phone: {companyDetails.ContactPhone}");
                        if (contactInfo.Any())
                        {
                            column.Item().Text(string.Join(" | ", contactInfo)).FontSize(9);
                        }

                        // Dynamic Country-Specific Identifiers
                        if (companyDetails.IdentifierFields != null && companyDetails.IdentifierFields.Any(f => !string.IsNullOrWhiteSpace(f.Value)))
                        {
                            var identifierInfo = new List<string>();
                            foreach (var identifier in companyDetails.IdentifierFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)).OrderBy(f => f.DisplayOrder))
                            {
                                identifierInfo.Add($"{identifier.DisplayName}: {identifier.Value}");
                            }
                            if (identifierInfo.Any())
                            {
                                column.Item().PaddingTop(3).Text(string.Join(" | ", identifierInfo)).FontSize(9);
                            }
                        }
                        else
                        {
                            // Fallback to legacy tax information
                            var taxInfo = new List<string>();
                            if (!string.IsNullOrWhiteSpace(companyDetails.PanNumber)) taxInfo.Add($"PAN: {companyDetails.PanNumber}");
                            if (!string.IsNullOrWhiteSpace(companyDetails.GstNumber)) taxInfo.Add($"GST: {companyDetails.GstNumber}");
                            if (taxInfo.Any())
                            {
                                column.Item().Text(string.Join(" | ", taxInfo)).FontSize(9);
                            }
                        }
                    }
                });

                // Quotation Info (Right) - 40% width
                row.RelativeItem(2).Column(column =>
                {
                    column.Item().AlignRight().Text("QUOTATION").FontSize(24).Bold().FontColor(Colors.Green.Darken3);
                    column.Item().AlignRight().Text($"#{quotation.QuotationNumber}").FontSize(16).Bold();
                    column.Item().AlignRight().PaddingTop(5).Text("Date: " + quotation.QuotationDate.ToString("dd MMM yyyy")).FontSize(10);
                    column.Item().AlignRight().Text("Valid Until: " + quotation.ValidUntil.ToString("dd MMM yyyy")).FontSize(10);
                });
            });
        }

        private static void Content(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Client Information (Bill To)
                column.Item().PaddingBottom(10).Column(clientColumn =>
                {
                    clientColumn.Item().Text("Bill To:").FontSize(12).Bold();
                    
                    if (quotation.Client != null)
                    {
                        // Company Name
                        if (!string.IsNullOrWhiteSpace(quotation.Client.CompanyName))
                        {
                            clientColumn.Item().Text(quotation.Client.CompanyName).FontSize(10);
                        }
                        
                        // Contact Name (if available)
                        if (!string.IsNullOrWhiteSpace(quotation.Client.ContactName))
                        {
                            clientColumn.Item().Text($"Contact: {quotation.Client.ContactName}").FontSize(9);
                        }
                        
                        // Address
                        if (!string.IsNullOrWhiteSpace(quotation.Client.Address))
                        {
                            clientColumn.Item().Text(quotation.Client.Address).FontSize(9);
                        }
                        
                        // City, State, PinCode
                        var addressParts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(quotation.Client.City)) addressParts.Add(quotation.Client.City);
                        if (!string.IsNullOrWhiteSpace(quotation.Client.State)) addressParts.Add(quotation.Client.State);
                        if (!string.IsNullOrWhiteSpace(quotation.Client.PinCode)) addressParts.Add(quotation.Client.PinCode);
                        if (addressParts.Any())
                        {
                            clientColumn.Item().Text(string.Join(", ", addressParts)).FontSize(9);
                        }
                        
                        // Email
                        if (!string.IsNullOrWhiteSpace(quotation.Client.Email))
                        {
                            clientColumn.Item().Text($"Email: {quotation.Client.Email}").FontSize(9);
                        }
                        
                        // Mobile/Phone
                        if (!string.IsNullOrWhiteSpace(quotation.Client.Mobile))
                        {
                            var phoneDisplay = quotation.Client.Mobile;
                            if (!string.IsNullOrWhiteSpace(quotation.Client.PhoneCode))
                            {
                                phoneDisplay = $"{quotation.Client.PhoneCode} {quotation.Client.Mobile}";
                            }
                            clientColumn.Item().Text($"Phone: {phoneDisplay}").FontSize(9);
                        }
                        
                        // GSTIN (if available)
                        if (!string.IsNullOrWhiteSpace(quotation.Client.Gstin))
                        {
                            clientColumn.Item().Text($"GSTIN: {quotation.Client.Gstin}").FontSize(9);
                        }
                    }
                    else
                    {
                        clientColumn.Item().Text("N/A").FontSize(10);
                    }
                });

                // Line Items Table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Item Name
                        columns.RelativeColumn(2); // Description
                        columns.ConstantColumn(60); // Quantity
                        columns.ConstantColumn(80); // Rate
                        columns.ConstantColumn(100); // Amount
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Item Name").Bold();
                        header.Cell().Element(CellStyle).Text("Description").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Rate").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Amount").Bold();
                    });

                    // Rows
                    foreach (var item in quotation.LineItems.OrderBy(x => x.SequenceNumber))
                    {
                        table.Cell().Element(CellStyle).Text(item.ItemName);
                        table.Cell().Element(CellStyle).Text(item.Description ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(item.UnitRate.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Amount.ToString("N2"));
                    }
                });

                // Summary
                column.Item().PaddingTop(20).Column(summaryColumn =>
                {
                    summaryColumn.Item().Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(200).Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal:");
                                r.ConstantItem(100).AlignRight().Text(quotation.SubTotal.ToString("N2"));
                            });
                            if (quotation.DiscountAmount > 0)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text($"Discount ({quotation.DiscountPercentage}%):");
                                    r.ConstantItem(100).AlignRight().Text($"-{quotation.DiscountAmount.ToString("N2")}");
                                });
                            }

                            // Display tax breakdown - prefer new framework-based breakdown, fallback to legacy fields
                            var taxBreakdown = GetTaxBreakdown(quotation);
                            if (taxBreakdown != null && taxBreakdown.Count > 0)
                            {
                                // New framework-based tax breakdown
                                foreach (var taxComponent in taxBreakdown)
                                {
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text($"{taxComponent.Component} ({taxComponent.Rate}%):");
                                        r.ConstantItem(100).AlignRight().Text(taxComponent.Amount.ToString("N2"));
                                    });
                                }
                            }
                            else
                            {
                                // Legacy tax fields (backward compatibility)
                                if (quotation.CgstAmount > 0)
                                {
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("CGST (9%):");
                                        r.ConstantItem(100).AlignRight().Text(quotation.CgstAmount.Value.ToString("N2"));
                                    });
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("SGST (9%):");
                                        r.ConstantItem(100).AlignRight().Text(quotation.SgstAmount?.ToString("N2") ?? "0.00");
                                    });
                                }
                                if (quotation.IgstAmount > 0)
                                {
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("IGST (18%):");
                                        r.ConstantItem(100).AlignRight().Text(quotation.IgstAmount.Value.ToString("N2"));
                                    });
                                }
                            }
                            col.Item().PaddingTop(5).BorderTop(1).Row(r =>
                            {
                                r.RelativeItem().Text("Total Amount:").Bold().FontSize(12);
                                r.ConstantItem(100).AlignRight().Text(quotation.TotalAmount.ToString("N2")).Bold().FontSize(12);
                            });
                        });
                    });
                });

                // Notes
                if (!string.IsNullOrWhiteSpace(quotation.Notes))
                {
                    column.Item().PaddingTop(20).Column(notesColumn =>
                    {
                        notesColumn.Item().Text("Notes:").Bold();
                        notesColumn.Item().Text(quotation.Notes);
                    });
                }

                // Company Details Section
                if (companyDetails != null)
                {
                    column.Item().PaddingTop(30).Column(companyColumn =>
                    {
                        companyColumn.Item().Text("Company Information").FontSize(12).Bold();
                        
                        // Company Address
                        if (!string.IsNullOrWhiteSpace(companyDetails.CompanyAddress))
                        {
                            companyColumn.Item().Text(companyDetails.CompanyAddress);
                        }
                        var addressParts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(companyDetails.City)) addressParts.Add(companyDetails.City);
                        if (!string.IsNullOrWhiteSpace(companyDetails.State)) addressParts.Add(companyDetails.State);
                        if (!string.IsNullOrWhiteSpace(companyDetails.PostalCode)) addressParts.Add(companyDetails.PostalCode);
                        if (!string.IsNullOrWhiteSpace(companyDetails.Country)) addressParts.Add(companyDetails.Country);
                        if (addressParts.Any())
                        {
                            companyColumn.Item().Text(string.Join(", ", addressParts));
                        }

                        // Contact Info
                        if (!string.IsNullOrWhiteSpace(companyDetails.ContactEmail))
                        {
                            companyColumn.Item().Text($"Email: {companyDetails.ContactEmail}");
                        }
                        if (!string.IsNullOrWhiteSpace(companyDetails.ContactPhone))
                        {
                            companyColumn.Item().Text($"Phone: {companyDetails.ContactPhone}");
                        }
                        if (!string.IsNullOrWhiteSpace(companyDetails.Website))
                        {
                            companyColumn.Item().Text($"Website: {companyDetails.Website}");
                        }

                        // Dynamic Country-Specific Identifiers
                        if (companyDetails.IdentifierFields != null && companyDetails.IdentifierFields.Any(f => !string.IsNullOrWhiteSpace(f.Value)))
                        {
                            companyColumn.Item().PaddingTop(5).Text("Company Identifiers:").FontSize(11).Bold();
                            foreach (var identifier in companyDetails.IdentifierFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)).OrderBy(f => f.DisplayOrder))
                            {
                                companyColumn.Item().Text($"{identifier.DisplayName}: {identifier.Value}");
                            }
                        }
                        else
                        {
                            // Fallback to legacy tax information
                            var taxInfo = new List<string>();
                            if (!string.IsNullOrWhiteSpace(companyDetails.PanNumber)) taxInfo.Add($"PAN: {companyDetails.PanNumber}");
                            if (!string.IsNullOrWhiteSpace(companyDetails.TanNumber)) taxInfo.Add($"TAN: {companyDetails.TanNumber}");
                            if (!string.IsNullOrWhiteSpace(companyDetails.GstNumber)) taxInfo.Add($"GST: {companyDetails.GstNumber}");
                            if (taxInfo.Any())
                            {
                                companyColumn.Item().PaddingTop(5).Text(string.Join(" | ", taxInfo));
                            }
                        }

                        // Dynamic Country-Specific Bank Details
                        if (companyDetails.BankFields != null && companyDetails.BankFields.Any(f => !string.IsNullOrWhiteSpace(f.Value)))
                        {
                            companyColumn.Item().PaddingTop(10).Column(bankColumn =>
                            {
                                bankColumn.Item().Text("Bank Details").FontSize(11).Bold();
                                
                                // Display bank fields dynamically
                                foreach (var bankField in companyDetails.BankFields.Where(f => !string.IsNullOrWhiteSpace(f.Value)).OrderBy(f => f.DisplayOrder))
                                {
                                    bankColumn.Item().Text($"{bankField.DisplayName}: {bankField.Value}");
                                }
                                
                                // Also show legacy bank details if available
                                if (companyDetails.BankDetails != null && companyDetails.BankDetails.Any())
                                {
                                    var bankDetails = companyDetails.BankDetails.FirstOrDefault();
                                    if (bankDetails != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(bankDetails.BankName))
                                        {
                                            bankColumn.Item().PaddingTop(3).Text($"Bank: {bankDetails.BankName}");
                                        }
                                        if (!string.IsNullOrWhiteSpace(bankDetails.BranchName))
                                        {
                                            bankColumn.Item().Text($"Branch: {bankDetails.BranchName}");
                                        }
                                        if (!string.IsNullOrWhiteSpace(bankDetails.AccountNumber))
                                        {
                                            bankColumn.Item().Text($"Account Number: {bankDetails.AccountNumber}");
                                        }
                                    }
                                }
                            });
                        }
                        else if (quotation.Client != null && companyDetails.BankDetails != null && companyDetails.BankDetails.Any())
                        {
                            // Fallback to legacy bank details
                            var bankDetails = companyDetails.BankDetails.FirstOrDefault();
                            if (bankDetails != null)
                            {
                                companyColumn.Item().PaddingTop(10).Column(bankColumn =>
                                {
                                    bankColumn.Item().Text("Bank Details").FontSize(11).Bold();
                                    bankColumn.Item().Text($"Bank: {bankDetails.BankName}");
                                    if (!string.IsNullOrWhiteSpace(bankDetails.BranchName))
                                    {
                                        bankColumn.Item().Text($"Branch: {bankDetails.BranchName}");
                                    }
                                    bankColumn.Item().Text($"Account Number: {bankDetails.AccountNumber}");
                                    if (!string.IsNullOrWhiteSpace(bankDetails.IfscCode))
                                    {
                                        bankColumn.Item().Text($"IFSC Code: {bankDetails.IfscCode}");
                                    }
                                    if (!string.IsNullOrWhiteSpace(bankDetails.Iban))
                                    {
                                        bankColumn.Item().Text($"IBAN: {bankDetails.Iban}");
                                    }
                                    if (!string.IsNullOrWhiteSpace(bankDetails.SwiftCode))
                                    {
                                        bankColumn.Item().Text($"SWIFT Code: {bankDetails.SwiftCode}");
                                    }
                                });
                            }
                        }

                        // Legal Disclaimer
                        if (!string.IsNullOrWhiteSpace(companyDetails.LegalDisclaimer))
                        {
                            companyColumn.Item().PaddingTop(10).Text(companyDetails.LegalDisclaimer).FontSize(8).Italic();
                        }
                    });
                }
            });
        }

        private static void Footer(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Thank you for your business!");
                text.AlignCenter();
            });
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(2);
        }

        private string BuildCacheKey(Quotation quotation) =>
            $"quotation-pdf-{quotation.QuotationId}-{quotation.UpdatedAt:O}";

        private TimeSpan? GetCacheDuration()
        {
            if (_settings.PdfCacheHours <= 0)
            {
                return null;
            }

            return TimeSpan.FromHours(_settings.PdfCacheHours);
        }

        private static List<TaxComponentBreakdown>? GetTaxBreakdown(Quotation quotation)
        {
            if (string.IsNullOrWhiteSpace(quotation.TaxBreakdown))
            {
                return null;
            }

            try
            {
                var breakdown = JsonSerializer.Deserialize<List<TaxComponentBreakdown>>(quotation.TaxBreakdown);
                return breakdown;
            }
            catch
            {
                return null;
            }
        }

        private class TaxComponentBreakdown
        {
            public string Component { get; set; } = string.Empty;
            public decimal Rate { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
