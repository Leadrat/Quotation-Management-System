using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Common.Services;
using CRM.Application.CompanyDetails.Dtos;
using CRM.Application.CompanyDetails.Services;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using System.Text.RegularExpressions;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.Content;
using PdfSharpCore.Pdf.Content.Objects;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.Annotations;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;
using UglyToad.PdfPig.Content;
using DocumentFormat.OpenXml.Packaging;
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using WordBody = DocumentFormat.OpenXml.Wordprocessing.Body;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace CRM.Application.QuotationTemplates.Services
{
    /// <summary>
    /// Service for processing file-based templates by replacing placeholders with actual quotation data
    /// </summary>
    public class TemplateProcessingService : ITemplateProcessingService
    {
        private readonly IAppDbContext _db;
        private readonly IFileStorageService _fileStorage;
        private readonly ICompanyDetailsService _companyDetailsService;
        private readonly ILogger<TemplateProcessingService> _logger;

        public TemplateProcessingService(
            IAppDbContext db,
            IFileStorageService fileStorage,
            ICompanyDetailsService companyDetailsService,
            ILogger<TemplateProcessingService> logger)
        {
            _db = db;
            _fileStorage = fileStorage;
            _companyDetailsService = companyDetailsService;
            _logger = logger;
        }

        public async Task<byte[]> ProcessTemplateToPdfAsync(QuotationTemplate template, Quotation quotation)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (quotation == null)
                throw new ArgumentNullException(nameof(quotation));
            if (!template.IsFileBased || string.IsNullOrWhiteSpace(template.FileUrl))
                throw new InvalidOperationException("Template is not file-based or file URL is missing");

            _logger.LogInformation("Processing template {TemplateId} (File: {FileName}, Type: {MimeType}) for quotation {QuotationId}", 
                template.TemplateId, template.FileName, template.MimeType, quotation.QuotationId);

            // Load quotation with all related data
            var fullQuotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .Include(q => q.CreatedByUser)
                .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

            if (fullQuotation == null)
                throw new InvalidOperationException($"Quotation {quotation.QuotationId} not found");

            // Ensure line items ordered
            if (fullQuotation?.LineItems != null)
            {
                fullQuotation.LineItems = fullQuotation.LineItems
                    .OrderBy(li => li.SequenceNumber)
                    .ToList();
            }

            // Ensure line items ordered
            if (fullQuotation?.LineItems != null)
            {
                fullQuotation.LineItems = fullQuotation.LineItems
                    .OrderBy(li => li.SequenceNumber)
                    .ToList();
            }

            // Get company details
            CompanyDetailsDto? companyDetails = null;
            if (!string.IsNullOrWhiteSpace(fullQuotation.CompanyDetailsSnapshot))
            {
                companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(fullQuotation.CompanyDetailsSnapshot);
            }
            if (companyDetails == null)
            {
                companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
            }

            // Read template file
            _logger.LogInformation("Reading template file from {FileUrl}", template.FileUrl);
            var templateBytes = await ReadTemplateFileAsync(template.FileUrl);
            _logger.LogInformation("Template file read successfully. Size: {Size} bytes", templateBytes.Length);
            
            var mimeType = template.MimeType?.ToLowerInvariant() ?? "";
            var fileName = template.FileName?.ToLowerInvariant() ?? "";

            // Process based on file type
            byte[] pdfBytes;
            if (mimeType.Contains("word") || fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || 
                fileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Processing Word template: {FileName}", template.FileName);
                pdfBytes = await ProcessWordTemplateAsync(templateBytes, fullQuotation, companyDetails);
            }
            else if (mimeType.Contains("html") || fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || 
                     fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Processing HTML template: {FileName}", template.FileName);
                pdfBytes = await ProcessHtmlTemplateAsync(templateBytes, fullQuotation, companyDetails);
            }
            else if (mimeType.Contains("pdf") || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Processing PDF template: {FileName}. Note: PDF templates will be recreated with data - original formatting may not be fully preserved.", template.FileName);
                // For PDF templates, we'll use QuestPDF to recreate from the template structure
                // This is a simplified approach - for complex PDFs, you might need iTextSharp or similar
                pdfBytes = await ProcessPdfTemplateAsync(templateBytes, fullQuotation, companyDetails);
            }
            else
            {
                _logger.LogError("Unsupported template file type. MimeType: {MimeType}, FileName: {FileName}", mimeType, template.FileName);
                throw new NotSupportedException($"Template file type {mimeType} (file: {template.FileName}) is not supported for processing. Supported types: PDF, Word (.doc, .docx), HTML (.html, .htm)");
            }
            
            _logger.LogInformation("Template processing completed. Generated PDF size: {Size} bytes", pdfBytes.Length);

            _logger.LogInformation("Successfully processed template {TemplateId} to PDF. Size: {Size} bytes", 
                template.TemplateId, pdfBytes.Length);

            return pdfBytes;
        }

        private async Task<byte[]> ReadTemplateFileAsync(string fileUrl)
        {
            try
            {
                // Remove leading slash if present
                var cleanPath = fileUrl.TrimStart('/');
                
                // If path starts with "uploads/", we need to construct the full path
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullPath = Path.Combine(basePath, cleanPath);

                if (!File.Exists(fullPath))
                {
                    // Try alternative path construction
                    if (cleanPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                    {
                        fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanPath);
                    }
                    else
                    {
                        fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", cleanPath);
                    }
                }

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"Template file not found at: {fullPath}");
                }

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read template file from {FileUrl}", fileUrl);
                throw;
            }
        }

        private async Task<byte[]> ProcessWordTemplateAsync(byte[] templateBytes, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            _logger.LogInformation("Processing Word template using OpenXML. Replacing placeholders in Word document.");
            
            return await Task.Run(() =>
            {
                byte[] processedWordBytes;
                
                // Step 1: Replace placeholders in Word document
                using (var stream = new MemoryStream(templateBytes))
                {
                    using (var wordDocument = WordprocessingDocument.Open(stream, true))
                    {
                        var replacements = BuildReplacementDictionary(quotation, companyDetails);
                        
                        // Get the main document part
                        var mainPart = wordDocument.MainDocumentPart;
                        if (mainPart == null)
                        {
                            throw new InvalidOperationException("Word document does not have a main document part");
                        }
                        
                        // Get the document body
                        var body = mainPart.Document?.Body;
                        if (body == null)
                        {
                            throw new InvalidOperationException("Word document body is null");
                        }
                        
                        // Replace placeholders in all text elements
                        ReplacePlaceholdersInWordDocument(body, replacements);
                        
                        // Save changes
                        if (mainPart.Document != null)
                        {
                            mainPart.Document.Save();
                        }
                        
                        // Get the processed document bytes
                        stream.Position = 0;
                        processedWordBytes = stream.ToArray();
                    }
                }
                
                // Step 2: Convert processed Word document to PDF
                // Extract content and render with proper structure
                _logger.LogInformation("Converting processed Word document to PDF while preserving structure");
                return ConvertWordToPdfAsync(processedWordBytes, quotation, companyDetails).Result;
            });
        }

        private void ReplacePlaceholdersInWordDocument(WordBody body, Dictionary<string, string> replacements)
        {
            foreach (var paragraph in body.Elements<WordParagraph>())
            {
                foreach (var run in paragraph.Elements<WordRun>())
                {
                    foreach (var text in run.Elements<WordText>())
                    {
                        var originalText = text.Text;
                        var replacedText = ReplacePlaceholdersInText(originalText, replacements);
                        if (replacedText != originalText)
                        {
                            text.Text = replacedText;
                            _logger.LogDebug("Replaced text in Word: '{Original}' -> '{Replaced}'", originalText, replacedText);
                        }
                    }
                }
            }
        }

        private string ReplacePlaceholdersInText(string text, Dictionary<string, string> replacements)
        {
            foreach (var replacement in replacements)
            {
                // Replace {Placeholder} and {{Placeholder}} formats
                text = Regex.Replace(text, 
                    $@"\{{{{?{Regex.Escape(replacement.Key)}\}}?\}}", 
                    replacement.Value ?? "", 
                    RegexOptions.IgnoreCase);
            }
            return text;
        }

        private async Task<byte[]> ConvertWordToPdfAsync(byte[] wordBytes, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            // For Word to PDF conversion, we'll extract the text content and render it
            // This is a simplified approach - for full formatting preservation, consider using a Word to PDF converter library
            
            _logger.LogInformation("Extracting content from Word document for PDF conversion");
            
            string documentText = "";
            using (var stream = new MemoryStream(wordBytes))
            using (var wordDocument = WordprocessingDocument.Open(stream, false))
            {
                var mainPart = wordDocument.MainDocumentPart;
                if (mainPart?.Document?.Body != null)
                {
                    documentText = ExtractTextFromWordBody(mainPart.Document.Body);
                }
            }
            
            // Replace placeholders again (in case they weren't replaced in the previous step)
            documentText = ReplacePlaceholders(documentText, quotation, companyDetails);
            
            // Convert to PDF using QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
            
            return await Task.Run(() =>
            {
                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Inch);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content().Padding(20).Column(column =>
                        {
                            // Split by paragraphs and render
                            var paragraphs = documentText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var para in paragraphs)
                            {
                                if (!string.IsNullOrWhiteSpace(para))
                                {
                                    column.Item().PaddingBottom(5).Text(para.Trim());
                                }
                            }
                        });
                    });
                }).GeneratePdf();
            });
        }

        public async Task<byte[]> ProcessTemplateToDocxAsync(QuotationTemplate template, Quotation quotation)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (quotation == null)
                throw new ArgumentNullException(nameof(quotation));
            if (!template.IsFileBased || string.IsNullOrWhiteSpace(template.FileUrl))
                throw new InvalidOperationException("Template is not file-based or file URL is missing");

            var mimeType = template.MimeType?.ToLowerInvariant() ?? string.Empty;
            var fileName = template.FileName?.ToLowerInvariant() ?? string.Empty;
            if (!(mimeType.Contains("word") || fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException("Only Word (.docx/.doc) templates are supported for DOCX generation");
            }

            // Ensure quotation has all related data loaded
            var fullQuotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .Include(q => q.CreatedByUser)
                .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId) ?? quotation;

            if (fullQuotation?.LineItems != null)
            {
                fullQuotation.LineItems = fullQuotation.LineItems
                    .OrderBy(li => li.SequenceNumber)
                    .ToList();
            }

            var templateBytes = await ReadTemplateFileAsync(template.FileUrl);

            using (var stream = new MemoryStream(templateBytes))
            using (var wordDocument = WordprocessingDocument.Open(stream, true))
            {
                // Load company details similar to PDF generation (prefer snapshot)
                CompanyDetailsDto? companyDetails = null;
                if (!string.IsNullOrWhiteSpace(fullQuotation.CompanyDetailsSnapshot))
                {
                    try { companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(fullQuotation.CompanyDetailsSnapshot); } catch { }
                }
                if (companyDetails == null)
                {
                    companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
                }

                var replacements = BuildReplacementDictionary(fullQuotation, companyDetails);
                var mainPart = wordDocument.MainDocumentPart ?? throw new InvalidOperationException("Word document does not have a main document part");
                var body = mainPart.Document?.Body ?? throw new InvalidOperationException("Word document body is null");
                ReplacePlaceholdersInWordDocument(body, replacements);
                mainPart.Document?.Save();
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        public async Task<byte[]> GenerateQuotationDocxAsync(Quotation quotation)
        {
            // Generates a simple DOCX with key quotation details and a basic items table
            var ms = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new WordDocument(new WordBody());

                var body = mainPart.Document.Body!;

                // Company Information (from snapshot or service)
                CompanyDetailsDto? companyDetails = null;
                if (!string.IsNullOrWhiteSpace(quotation.CompanyDetailsSnapshot))
                {
                    try { companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(quotation.CompanyDetailsSnapshot); } catch { }
                }
                if (companyDetails == null)
                {
                    try { companyDetails = await _companyDetailsService.GetCompanyDetailsAsync(); } catch { }
                }

                if (companyDetails != null)
                {
                    body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(companyDetails.CompanyName ?? ""))));
                    if (!string.IsNullOrWhiteSpace(companyDetails.CompanyAddress))
                        body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(companyDetails.CompanyAddress))));
                    if (!string.IsNullOrWhiteSpace(companyDetails.ContactEmail))
                        body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Email: {companyDetails.ContactEmail}"))));
                    if (!string.IsNullOrWhiteSpace(companyDetails.ContactPhone))
                        body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Phone: {companyDetails.ContactPhone}"))));
                }

                // Header
                body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Quotation #{quotation.QuotationNumber}"))));
                body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Date: {quotation.QuotationDate:dd MMM yyyy}"))));
                body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Valid Until: {quotation.ValidUntil:dd MMM yyyy}"))));

                // Client
                if (quotation.Client != null)
                {
                    body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Client: {quotation.Client.CompanyName}"))));
                    if (!string.IsNullOrWhiteSpace(quotation.Client.Email))
                        body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Email: {quotation.Client.Email}"))));
                    if (!string.IsNullOrWhiteSpace(quotation.Client.Address))
                        body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(quotation.Client.Address))));
                    var addrParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(quotation.Client.City)) addrParts.Add(quotation.Client.City);
                    if (!string.IsNullOrWhiteSpace(quotation.Client.State)) addrParts.Add(quotation.Client.State);
                    if (!string.IsNullOrWhiteSpace(quotation.Client.PinCode)) addrParts.Add(quotation.Client.PinCode);
                    if (addrParts.Count > 0)
                        body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(string.Join(", ", addrParts)))));
                }

                // Line items table
                if (quotation.LineItems != null && quotation.LineItems.Any())
                {
                    var table = new DocumentFormat.OpenXml.Wordprocessing.Table();
                    var props = new DocumentFormat.OpenXml.Wordprocessing.TableProperties(
                        new DocumentFormat.OpenXml.Wordprocessing.TableBorders(
                            new DocumentFormat.OpenXml.Wordprocessing.TopBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.Single, Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.BottomBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.Single, Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.LeftBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.Single, Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.RightBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.Single, Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.Single, Size = 4 },
                            new DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder { Val = DocumentFormat.OpenXml.Wordprocessing.BorderValues.Single, Size = 4 }
                        ));
                    table.AppendChild(props);

                    // Header row
                    var headerRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                    headerRow.Append(
                        MakeCell("Item"),
                        MakeCell("Description"),
                        MakeCell("Qty"),
                        MakeCell("Rate"),
                        MakeCell("Amount")
                    );
                    table.AppendChild(headerRow);

                    foreach (var li in quotation.LineItems.OrderBy(x => x.SequenceNumber))
                    {
                        var row = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        row.Append(
                            MakeCell(li.ItemName),
                            MakeCell(li.Description ?? string.Empty),
                            MakeCell(li.Quantity.ToString("N2")),
                            MakeCell(li.UnitRate.ToString("N2")),
                            MakeCell(li.Amount.ToString("N2"))
                        );
                        table.AppendChild(row);
                    }

                    body.AppendChild(table);
                }

                // Totals
                body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Subtotal: {quotation.SubTotal:N2}"))));
                if (quotation.DiscountAmount > 0)
                {
                    body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Discount ({quotation.DiscountPercentage}%): -{quotation.DiscountAmount:N2}"))));
                }
                body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Tax: {quotation.TaxAmount:N2}"))));
                body.AppendChild(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Total: {quotation.TotalAmount:N2}"))));

                mainPart.Document.Save();
            }
            ms.Position = 0;
            return ms.ToArray();

            DocumentFormat.OpenXml.Wordprocessing.TableCell MakeCell(string text)
            {
                var cell = new DocumentFormat.OpenXml.Wordprocessing.TableCell();
                var para = new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text(text)));
                cell.Append(para);
                return cell;
            }
        }

        private string ExtractTextFromWordBody(WordBody body)
        {
            var sb = new StringBuilder();
            foreach (var paragraph in body.Elements<WordParagraph>())
            {
                foreach (var run in paragraph.Elements<WordRun>())
                {
                    foreach (var text in run.Elements<WordText>())
                    {
                        sb.Append(text.Text);
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private async Task<byte[]> ProcessHtmlTemplateAsync(byte[] templateBytes, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            _logger.LogInformation("Processing HTML template. Reading HTML content and replacing placeholders.");
            
            var content = Encoding.UTF8.GetString(templateBytes);
            _logger.LogDebug("HTML template content length: {Length} characters", content.Length);
            
            // Replace placeholders
            _logger.LogInformation("Replacing placeholders in HTML template");
            content = ReplacePlaceholders(content, quotation, companyDetails);
            _logger.LogDebug("HTML content after placeholder replacement length: {Length} characters", content.Length);
            
            // Convert HTML to PDF with proper rendering
            _logger.LogInformation("Converting HTML to PDF with structure preservation");
            return await ConvertHtmlToPdfAsync(content);
        }

        private async Task<byte[]> ProcessPdfTemplateAsync(byte[] templateBytes, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            _logger.LogInformation("Processing PDF template. Will generate NEW PDF with actual quotation data (client: {ClientName}, items: {ItemCount}, total: {Total})", 
                quotation.Client?.CompanyName ?? "N/A", quotation.LineItems?.Count ?? 0, quotation.TotalAmount);
            
            // IMPORTANT: Always generate a NEW PDF with actual quotation data
            // We use the template only to determine page count and structure
            // This ensures the client NEVER receives the template with placeholders
            
            return await Task.Run(() =>
            {
                try
                {
                    // Try to extract page count and structure from template
                    int pageCount = 1;
                    try
                    {
                        using (var pdfDocument = PdfPigDocument.Open(templateBytes))
                        {
                            pageCount = pdfDocument.NumberOfPages;
                            _logger.LogInformation("Template has {PageCount} pages. Will generate new PDF with same page count using actual quotation data.", pageCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not read template PDF structure, using default 1 page. Will generate new PDF with actual quotation data.");
                    }
                    
                    // ALWAYS generate a new PDF with actual quotation data
                    // This ensures placeholders are replaced with real data
                    _logger.LogInformation("Generating NEW PDF with actual quotation data for client {ClientName}, {ItemCount} line items, total {Total}", 
                        quotation.Client?.CompanyName ?? "N/A", quotation.LineItems?.Count ?? 0, quotation.TotalAmount);
                    
                    return GenerateNewPdfFromTemplate(templateBytes, quotation, companyDetails);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PDF generation. Error: {Error}. Generating fallback PDF with actual quotation data.", ex.Message);
                    // Even on error, generate new PDF - NEVER return template
                    return GenerateNewPdfFromTemplate(templateBytes, quotation, companyDetails);
                }
            });
        }


        private byte[] GenerateNewPdfFromTemplate(byte[] templateBytes, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            _logger.LogInformation("Generating NEW PDF with actual quotation data. Template will be used only for structure reference.");
            
            // Determine page count from template
            int pageCount = 1;
            try
            {
                using (var templateStream = new MemoryStream(templateBytes))
                {
                    var templateDoc = PdfReader.Open(templateStream, PdfDocumentOpenMode.ReadOnly);
                    if (templateDoc != null)
                    {
                        pageCount = templateDoc.PageCount;
                        templateDoc.Close();
                        _logger.LogInformation("Template has {PageCount} pages. Generating new PDF with same page count.", pageCount);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine page count from template, using default 1 page.");
            }
            
            // Generate NEW PDF with actual quotation data using QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
            
            return Document.Create(container =>
            {
                for (int i = 0; i < pageCount; i++)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Inch);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        if (i == 0)
                        {
                            // First page has header with company logo and quotation info
                            page.Header().Element(c => RenderQuotationHeader(c, quotation, companyDetails));
                        }
                        
                        // Content with actual quotation data
                        page.Content().Element(c => RenderQuotationContent(c, quotation, companyDetails, i == 0));
                        
                        if (i == pageCount - 1)
                        {
                            // Last page has footer
                            page.Footer().Element(c => RenderQuotationFooter(c, quotation, companyDetails));
                        }
                    });
                }
            }).GeneratePdf();
        }

        private void RenderQuotationHeader(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.Row(row =>
            {
                // Company Info (Left)
                row.RelativeItem(3).Column(column =>
                {
                    if (companyDetails != null && !string.IsNullOrWhiteSpace(companyDetails.CompanyName))
                    {
                        column.Item().Text(companyDetails.CompanyName).FontSize(16).Bold();
                    }
                    if (companyDetails != null && !string.IsNullOrWhiteSpace(companyDetails.CompanyAddress))
                    {
                        column.Item().Text(companyDetails.CompanyAddress).FontSize(9);
                    }
                });

                // Quotation Info (Right)
                row.RelativeItem(2).Column(column =>
                {
                    column.Item().AlignRight().Text("QUOTATION").FontSize(24).Bold().FontColor(Colors.Green.Darken3);
                    column.Item().AlignRight().Text($"#{quotation.QuotationNumber}").FontSize(16).Bold();
                    column.Item().AlignRight().Text($"Date: {quotation.QuotationDate:dd MMM yyyy}").FontSize(10);
                    column.Item().AlignRight().Text($"Valid Until: {quotation.ValidUntil:dd MMM yyyy}").FontSize(10);
                });
            });
        }

        private void RenderQuotationContent(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails, bool isFirstPage)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                if (isFirstPage)
                {
                    // Client Information
                    column.Item().PaddingBottom(10).Column(clientColumn =>
                    {
                        clientColumn.Item().Text("Bill To:").FontSize(12).Bold();
                        if (quotation.Client != null)
                        {
                            if (!string.IsNullOrWhiteSpace(quotation.Client.CompanyName))
                            {
                                clientColumn.Item().Text(quotation.Client.CompanyName).FontSize(10);
                            }
                            if (!string.IsNullOrWhiteSpace(quotation.Client.Address))
                            {
                                clientColumn.Item().Text(quotation.Client.Address).FontSize(9);
                            }
                            var addressParts = new List<string>();
                            if (!string.IsNullOrWhiteSpace(quotation.Client.City)) addressParts.Add(quotation.Client.City);
                            if (!string.IsNullOrWhiteSpace(quotation.Client.State)) addressParts.Add(quotation.Client.State);
                            if (!string.IsNullOrWhiteSpace(quotation.Client.PinCode)) addressParts.Add(quotation.Client.PinCode);
                            if (addressParts.Any())
                            {
                                clientColumn.Item().Text(string.Join(", ", addressParts)).FontSize(9);
                            }
                        }
                    });

                    // Line Items Table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Item Name").Bold();
                            header.Cell().Element(CellStyle).Text("Description").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Rate").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Amount").Bold();
                        });

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
                                col.Item().Row(r =>
                                {
                                    r.RelativeItem().Text("Tax:");
                                    r.ConstantItem(100).AlignRight().Text(quotation.TaxAmount.ToString("N2"));
                                });
                                col.Item().PaddingTop(5).BorderTop(1).Row(r =>
                                {
                                    r.RelativeItem().Text("Total Amount:").Bold().FontSize(12);
                                    r.ConstantItem(100).AlignRight().Text(quotation.TotalAmount.ToString("N2")).Bold().FontSize(12);
                                });
                            });
                        });
                    });
                }

                // Notes
                if (!string.IsNullOrWhiteSpace(quotation.Notes))
                {
                    column.Item().PaddingTop(20).Column(notesColumn =>
                    {
                        notesColumn.Item().Text("Notes:").Bold();
                        notesColumn.Item().Text(quotation.Notes);
                    });
                }
            });
        }

        private void RenderQuotationFooter(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.AlignCenter().Text($"Valid Until: {quotation.ValidUntil:dd/MM/yyyy}").FontSize(8);
        }

        private byte[] CreatePdfFromTemplateStructure(byte[] templateBytes, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            // Determine page count from template
            int pageCount = 1;
            try
            {
                using (var templateStream = new MemoryStream(templateBytes))
                {
                    var templateDoc = PdfReader.Open(templateStream, PdfDocumentOpenMode.ReadOnly);
                    if (templateDoc != null)
                    {
                        pageCount = templateDoc.PageCount;
                        templateDoc.Close();
                    }
                }
            }
            catch
            {
                // Use default
            }
            
            _logger.LogInformation("Creating PDF with {PageCount} pages to match template structure", pageCount);
            
            QuestPDF.Settings.License = LicenseType.Community;
            
            return Document.Create(container =>
            {
                for (int i = 0; i < pageCount; i++)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Inch);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        if (i == 0)
                        {
                            // First page has header
                            page.Header().Element(c => RenderHeader(c, quotation, companyDetails));
                        }
                        
                        // Content
                        page.Content().Element(c => RenderContent(c, quotation, companyDetails));
                        
                        if (i == pageCount - 1)
                        {
                            // Last page has footer
                            page.Footer().Element(c => RenderFooter(c, quotation, companyDetails));
                        }
                    });
                }
            }).GeneratePdf();
        }

        private string ReplacePlaceholders(string content, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            var replacements = BuildReplacementDictionary(quotation, companyDetails);
            
            foreach (var replacement in replacements)
            {
                // Replace {Placeholder} and {{Placeholder}} formats
                content = Regex.Replace(content, 
                    $@"\{{{{?{Regex.Escape(replacement.Key)}\}}?\}}", 
                    replacement.Value ?? "", 
                    RegexOptions.IgnoreCase);
            }
            
            return content;
        }

        private Dictionary<string, string> BuildReplacementDictionary(Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            // Client information
            replacements["ClientName"] = quotation.Client?.CompanyName ?? "";
            replacements["ClientCompanyName"] = quotation.Client?.CompanyName ?? "";
            replacements["ClientAddress"] = FormatAddress(quotation.Client?.Address, quotation.Client?.City, quotation.Client?.State, quotation.Client?.PinCode);
            replacements["ClientCity"] = quotation.Client?.City ?? "";
            replacements["ClientState"] = quotation.Client?.State ?? "";
            replacements["ClientPincode"] = quotation.Client?.PinCode ?? "";
            replacements["ClientGSTIN"] = quotation.Client?.Gstin ?? "";
            replacements["ClientEmail"] = quotation.Client?.Email ?? "";
            var clientPhone = quotation.Client?.Mobile ?? "";
            if (!string.IsNullOrWhiteSpace(quotation.Client?.PhoneCode))
            {
                clientPhone = $"{quotation.Client.PhoneCode} {clientPhone}";
            }
            replacements["ClientPhone"] = clientPhone;
            
            // Pricing information (for templates with pricing tables)
            replacements["SubTotal"] = $"Rs. {quotation.SubTotal:N0}/-";
            replacements["SubTotalAmount"] = quotation.SubTotal.ToString("N0");
            replacements["DiscountAmount"] = $"Rs. {quotation.DiscountAmount:N0}/-";
            replacements["DiscountAmountValue"] = quotation.DiscountAmount.ToString("N0");
            replacements["TaxAmount"] = $"Rs. {quotation.TaxAmount:N0}/-";
            replacements["TaxAmountValue"] = quotation.TaxAmount.ToString("N0");
            replacements["TotalAmount"] = $"Rs. {quotation.TotalAmount:N0}/-";
            replacements["TotalAmountValue"] = quotation.TotalAmount.ToString("N0");
            replacements["TotalAmountWithGST"] = $"Rs. {quotation.TotalAmount:N0}/-";
            
            // Pricing in Indian format (with commas)
            replacements["SubTotalFormatted"] = $"Rs. {quotation.SubTotal:N0}/-";
            replacements["TotalAmountFormatted"] = $"Rs. {quotation.TotalAmount:N0}/-";
            
            // Line items summary (for templates that show item counts)
            replacements["LineItemCount"] = quotation.LineItems?.Count.ToString() ?? "0";
            replacements["ItemCount"] = quotation.LineItems?.Count.ToString() ?? "0";
            
            // Company information
            if (companyDetails != null)
            {
                replacements["CompanyName"] = companyDetails.CompanyName ?? "";
                replacements["CompanyAddress"] = FormatAddress(companyDetails.CompanyAddress, companyDetails.City, companyDetails.State, companyDetails.PostalCode);
                replacements["CompanyCity"] = companyDetails.City ?? "";
                replacements["CompanyState"] = companyDetails.State ?? "";
                replacements["CompanyPincode"] = companyDetails.PostalCode ?? "";
                replacements["CompanyGSTIN"] = companyDetails.GstNumber ?? "";
                replacements["CompanyEmail"] = companyDetails.ContactEmail ?? "";
                replacements["CompanyPhone"] = companyDetails.ContactPhone ?? "";
            }
            
            // Quotation information
            replacements["QuotationNumber"] = quotation.QuotationNumber;
            replacements["QuotationDate"] = quotation.QuotationDate.ToString("dd/MM/yyyy");
            replacements["ValidUntil"] = quotation.ValidUntil.ToString("dd/MM/yyyy");
            replacements["SubTotal"] = quotation.SubTotal.ToString("N2");
            replacements["DiscountPercentage"] = quotation.DiscountPercentage.ToString("N2");
            replacements["DiscountAmount"] = quotation.DiscountAmount.ToString("N2");
            replacements["TaxAmount"] = quotation.TaxAmount.ToString("N2");
            replacements["CgstAmount"] = quotation.CgstAmount?.ToString("N2") ?? "0.00";
            replacements["SgstAmount"] = quotation.SgstAmount?.ToString("N2") ?? "0.00";
            replacements["IgstAmount"] = quotation.IgstAmount?.ToString("N2") ?? "0.00";
            replacements["TotalAmount"] = quotation.TotalAmount.ToString("N2");
            replacements["Notes"] = quotation.Notes ?? "";
            
            // Line items
            var lineItemsHtml = GenerateLineItemsHtml(quotation.LineItems);
            replacements["LineItems"] = lineItemsHtml;
            
            // Current date
            replacements["CurrentDate"] = DateTime.Now.ToString("dd/MM/yyyy");
            replacements["CurrentDateTime"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            return replacements;
        }

        private string FormatAddress(string? address, string? city, string? state, string? pincode)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(address)) parts.Add(address);
            if (!string.IsNullOrWhiteSpace(city)) parts.Add(city);
            if (!string.IsNullOrWhiteSpace(state)) parts.Add(state);
            if (!string.IsNullOrWhiteSpace(pincode)) parts.Add(pincode);
            return string.Join(", ", parts);
        }

        private string FormatLineItemsForPdf(IEnumerable<QuotationLineItem> lineItems)
        {
            if (lineItems == null || !lineItems.Any())
            {
                return "No items";
            }
            
            var sb = new StringBuilder();
            sb.AppendLine("Item Name | Description | Qty | Rate | Amount");
            sb.AppendLine("---------------------------------------------------");
            
            foreach (var item in lineItems.OrderBy(li => li.SequenceNumber))
            {
                sb.AppendLine($"{item.ItemName} | {item.Description ?? ""} | {item.Quantity:N2} | {item.UnitRate:N2} | {item.Amount:N2}");
            }
            
            return sb.ToString();
        }

        private string GenerateLineItemsHtml(IEnumerable<QuotationLineItem> lineItems)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table style='width:100%; border-collapse: collapse;'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr style='background-color: #f0f0f0;'>");
            sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Item</th>");
            sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Description</th>");
            sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Quantity</th>");
            sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Rate</th>");
            sb.AppendLine("<th style='border: 1px solid #ddd; padding: 8px; text-align: right;'>Amount</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");
            
            foreach (var item in lineItems.OrderBy(li => li.SequenceNumber))
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{item.ItemName}</td>");
                sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px;'>{item.Description ?? ""}</td>");
                sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{item.Quantity.ToString("N2")}</td>");
                sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{item.UnitRate.ToString("N2")}</td>");
                sb.AppendLine($"<td style='border: 1px solid #ddd; padding: 8px; text-align: right;'>{item.Amount.ToString("N2")}</td>");
                sb.AppendLine("</tr>");
            }
            
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            return await Task.Run(() =>
            {
                // Parse HTML and render with proper structure
                // This approach preserves HTML structure (tables, formatting, etc.)
                
                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Detect page size from HTML or use A4
                        page.Size(PageSizes.A4);
                        page.Margin(0.5f, Unit.Inch);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content().Element(c => RenderHtmlContent(c, htmlContent));
                    });
                }).GeneratePdf();
            });
        }

        private void RenderHtmlContent(IContainer container, string htmlContent)
        {
            // Remove script and style tags
            htmlContent = Regex.Replace(htmlContent, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            htmlContent = Regex.Replace(htmlContent, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Extract body content if HTML document
            var bodyMatch = Regex.Match(htmlContent, @"<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (bodyMatch.Success)
            {
                htmlContent = bodyMatch.Groups[1].Value;
            }
            
            container.Padding(20).Column(column =>
            {
                // Parse and render HTML elements
                RenderHtmlElements(column, htmlContent);
            });
        }

        private void RenderHtmlElements(QuestPDF.Fluent.ColumnDescriptor column, string htmlContent)
        {
            // Split by block elements
            var blockElements = Regex.Split(htmlContent, @"(?=<(?:div|p|h[1-6]|table|ul|ol|li|br|hr)[^>]*>)", RegexOptions.IgnoreCase);
            
            foreach (var element in blockElements)
            {
                if (string.IsNullOrWhiteSpace(element)) continue;
                
                var trimmed = element.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                
                // Handle tables
                if (Regex.IsMatch(trimmed, @"<table", RegexOptions.IgnoreCase))
                {
                    RenderHtmlTable(column, trimmed);
                }
                // Handle headings
                else if (Regex.IsMatch(trimmed, @"<h[1-6]", RegexOptions.IgnoreCase))
                {
                    var headingMatch = Regex.Match(trimmed, @"<h([1-6])[^>]*>(.*?)</h\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (headingMatch.Success)
                    {
                        var level = int.Parse(headingMatch.Groups[1].Value);
                        var text = StripHtmlTags(headingMatch.Groups[2].Value);
                        var fontSize = 24 - (level * 2);
                        column.Item().PaddingBottom(5).Text(text).FontSize(fontSize).Bold();
                    }
                }
                // Handle paragraphs
                else if (Regex.IsMatch(trimmed, @"<p", RegexOptions.IgnoreCase))
                {
                    var paraMatch = Regex.Match(trimmed, @"<p[^>]*>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (paraMatch.Success)
                    {
                        var text = StripHtmlTags(paraMatch.Groups[1].Value);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            column.Item().PaddingBottom(3).Text(text);
                        }
                    }
                }
                // Handle line breaks
                else if (trimmed.Contains("<br", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = Regex.Split(trimmed, @"<br[^>]*/?>", RegexOptions.IgnoreCase);
                    foreach (var part in parts)
                    {
                        var text = StripHtmlTags(part);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            column.Item().PaddingBottom(2).Text(text);
                        }
                    }
                }
                // Handle divs
                else if (Regex.IsMatch(trimmed, @"<div", RegexOptions.IgnoreCase))
                {
                    var divMatch = Regex.Match(trimmed, @"<div[^>]*>(.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (divMatch.Success)
                    {
                        RenderHtmlElements(column, divMatch.Groups[1].Value);
                    }
                }
                // Plain text
                else
                {
                    var text = StripHtmlTags(trimmed);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        column.Item().PaddingBottom(2).Text(text);
                    }
                }
            }
        }

        private void RenderHtmlTable(QuestPDF.Fluent.ColumnDescriptor column, string tableHtml)
        {
            var tableMatch = Regex.Match(tableHtml, @"<table[^>]*>(.*?)</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!tableMatch.Success) return;
            
            var tableContent = tableMatch.Groups[1].Value;
            
            column.Item().PaddingVertical(10).Table(table =>
            {
                // Extract rows
                var rowMatches = Regex.Matches(tableContent, @"<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                if (rowMatches.Count == 0) return;
                
                // Determine column count from first row
                var firstRow = rowMatches[0].Groups[1].Value;
                var cellMatches = Regex.Matches(firstRow, @"<(?:th|td)[^>]*>(.*?)</(?:th|td)>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var columnCount = cellMatches.Count;
                
                if (columnCount == 0) return;
                
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        columns.RelativeColumn();
                    }
                });
                
                // Render rows
                bool isHeader = true;
                foreach (Match rowMatch in rowMatches)
                {
                    var rowContent = rowMatch.Groups[1].Value;
                    var cells = Regex.Matches(rowContent, @"<(?:th|td)[^>]*>(.*?)</(?:th|td)>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    
                    if (cells.Count != columnCount) continue;
                    
                    if (isHeader)
                    {
                        table.Header(header =>
                        {
                            foreach (Match cell in cells)
                            {
                                var cellText = StripHtmlTags(cell.Groups[1].Value);
                                header.Cell().Element(CellStyle).Text(cellText).Bold();
                            }
                        });
                        isHeader = false;
                    }
                    else
                    {
                        foreach (Match cell in cells)
                        {
                            var cellText = StripHtmlTags(cell.Groups[1].Value);
                            table.Cell().Element(CellStyle).Text(cellText);
                        }
                    }
                }
            });
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            
            // Decode HTML entities
            var decoded = System.Net.WebUtility.HtmlDecode(html);
            
            // Remove HTML tags
            var stripped = Regex.Replace(decoded, "<[^>]+>", " ");
            
            // Clean up whitespace
            stripped = Regex.Replace(stripped, @"\s+", " ").Trim();
            
            return stripped;
        }

        private IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
        }

        private void RenderHeader(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.Row(row =>
            {
                row.RelativeItem(3).Column(column =>
                {
                    if (companyDetails != null)
                    {
                        column.Item().Text(companyDetails.CompanyName ?? "").FontSize(16).Bold();
                        if (!string.IsNullOrWhiteSpace(companyDetails.CompanyAddress))
                        {
                            column.Item().Text(companyDetails.CompanyAddress).FontSize(9);
                        }
                        var addressLine = string.Join(", ", 
                            new[] { companyDetails.City, companyDetails.State, companyDetails.PostalCode }
                                .Where(x => !string.IsNullOrWhiteSpace(x)));
                        if (!string.IsNullOrWhiteSpace(addressLine))
                        {
                            column.Item().Text(addressLine).FontSize(9);
                        }
                    }
                });
                
                row.RelativeItem(2).AlignRight().Column(column =>
                {
                    column.Item().Text("QUOTATION").FontSize(20).Bold();
                    column.Item().Text($"No: {quotation.QuotationNumber}").FontSize(10);
                    column.Item().Text($"Date: {quotation.QuotationDate:dd/MM/yyyy}").FontSize(10);
                });
            });
        }

        private void RenderContent(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.Column(column =>
            {
                // Client section
                column.Item().PaddingBottom(10).Column(clientColumn =>
                {
                    clientColumn.Item().Text("To:").FontSize(10).Bold();
                    if (quotation.Client != null)
                    {
                        clientColumn.Item().Text(quotation.Client.CompanyName ?? "").FontSize(10);
                        var clientAddress = FormatAddress(quotation.Client.Address, quotation.Client.City, 
                            quotation.Client.State, quotation.Client.PinCode);
                        if (!string.IsNullOrWhiteSpace(clientAddress))
                        {
                            clientColumn.Item().Text(clientAddress).FontSize(9);
                        }
                    }
                });

                // Line items table
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Item").Bold();
                        header.Cell().Element(CellStyle).Text("Description").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Rate").Bold();
                        header.Cell().Element(CellStyle).AlignRight().Text("Amount").Bold();
                    });

                    foreach (var item in quotation.LineItems.OrderBy(li => li.SequenceNumber))
                    {
                        table.Cell().Element(CellStyle).Text(item.ItemName);
                        table.Cell().Element(CellStyle).Text(item.Description ?? "");
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(item.UnitRate.ToString("N2"));
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Amount.ToString("N2"));
                    }
                });

                // Totals
                column.Item().PaddingTop(20).AlignRight().Column(totalsColumn =>
                {
                    totalsColumn.Item().Row(row =>
                    {
                        row.RelativeItem(2).Text("Sub Total:").FontSize(10);
                        row.RelativeItem(1).AlignRight().Text(quotation.SubTotal.ToString("N2")).FontSize(10);
                    });
                    
                    if (quotation.DiscountPercentage > 0)
                    {
                        totalsColumn.Item().Row(row =>
                        {
                            row.RelativeItem(2).Text($"Discount ({quotation.DiscountPercentage}%):").FontSize(10);
                            row.RelativeItem(1).AlignRight().Text(quotation.DiscountAmount.ToString("N2")).FontSize(10);
                        });
                    }
                    
                    totalsColumn.Item().Row(row =>
                    {
                        row.RelativeItem(2).Text("Tax:").FontSize(10);
                        row.RelativeItem(1).AlignRight().Text(quotation.TaxAmount.ToString("N2")).FontSize(10);
                    });
                    
                    totalsColumn.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem(2).Text("Total:").FontSize(12).Bold();
                        row.RelativeItem(1).AlignRight().Text(quotation.TotalAmount.ToString("N2")).FontSize(12).Bold();
                    });
                });

                // Notes
                if (!string.IsNullOrWhiteSpace(quotation.Notes))
                {
                    column.Item().PaddingTop(20).Text("Notes:").FontSize(10).Bold();
                    column.Item().Text(quotation.Notes).FontSize(9);
                }
            });
        }

        private void RenderFooter(IContainer container, Quotation quotation, CompanyDetailsDto? companyDetails)
        {
            container.AlignCenter().Text($"Valid Until: {quotation.ValidUntil:dd/MM/yyyy}").FontSize(8);
        }

        public async Task<string> ExtractTextFromTemplateAsync(QuotationTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (!template.IsFileBased || string.IsNullOrWhiteSpace(template.FileUrl))
                throw new InvalidOperationException("Template is not file-based or file URL is missing");

            _logger.LogInformation("Extracting text from template {TemplateId} (File: {FileName}, Type: {MimeType})", 
                template.TemplateId, template.FileName, template.MimeType);

            // Read template file
            var templateBytes = await ReadTemplateFileAsync(template.FileUrl);
            var fileName = template.FileName?.ToLowerInvariant() ?? "";
            var mimeType = template.MimeType?.ToLowerInvariant() ?? "";

            // Extract text based on file type
            if (mimeType.Contains("word") || fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || 
                fileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractTextFromWordAsync(templateBytes);
            }
            else if (mimeType.Contains("html") || fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || 
                     fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractTextFromHtmlAsync(templateBytes);
            }
            else if (mimeType.Contains("pdf") || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return await ExtractTextFromPdfAsync(templateBytes);
            }
            else
            {
                _logger.LogWarning("Unsupported template file type for text extraction. MimeType: {MimeType}, FileName: {FileName}", mimeType, template.FileName);
                return "";
            }
        }

        private async Task<string> ExtractTextFromWordAsync(byte[] templateBytes)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var stream = new MemoryStream(templateBytes))
                    using (var wordDocument = WordprocessingDocument.Open(stream, false))
                    {
                        var mainPart = wordDocument.MainDocumentPart;
                        if (mainPart?.Document?.Body != null)
                        {
                            var text = ExtractTextFromWordBody(mainPart.Document.Body);
                            _logger.LogInformation("Extracted {Length} characters from Word template", text.Length);
                            return text;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting text from Word template");
                    return "";
                }
                return "";
            });
        }

        private async Task<string> ExtractTextFromHtmlAsync(byte[] templateBytes)
        {
            var content = Encoding.UTF8.GetString(templateBytes);
            
            // Remove script and style tags
            content = Regex.Replace(content, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            content = Regex.Replace(content, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Extract body content if HTML document
            var bodyMatch = Regex.Match(content, @"<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (bodyMatch.Success)
            {
                content = bodyMatch.Groups[1].Value;
            }
            
            // Strip HTML tags and decode entities
            var text = System.Net.WebUtility.HtmlDecode(Regex.Replace(content, "<[^>]+>", " "));
            
            // Clean up whitespace
            text = Regex.Replace(text, @"\s+", " ").Trim();
            
            _logger.LogInformation("Extracted {Length} characters from HTML template", text.Length);
            return await Task.FromResult(text);
        }

        private async Task<string> ExtractTextFromPdfAsync(byte[] templateBytes)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var sb = new StringBuilder();
                    
                    using (var pdfDocument = PdfPigDocument.Open(templateBytes))
                    {
                        _logger.LogInformation("Extracting text from PDF template. Pages: {PageCount}", pdfDocument.NumberOfPages);
                        
                        for (int pageIndex = 0; pageIndex < pdfDocument.NumberOfPages; pageIndex++)
                        {
                            var page = pdfDocument.GetPage(pageIndex + 1);
                            var words = page.GetWords();
                            
                            foreach (var word in words)
                            {
                                sb.Append(word.Text);
                                sb.Append(" ");
                            }
                            
                            // Add line break between pages
                            if (pageIndex < pdfDocument.NumberOfPages - 1)
                            {
                                sb.AppendLine();
                                sb.AppendLine();
                            }
                        }
                    }
                    
                    var text = sb.ToString().Trim();
                    _logger.LogInformation("Extracted {Length} characters from PDF template", text.Length);
                    return text;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting text from PDF template");
                    return "";
                }
            });
        }

        public async Task<string> ProcessTemplateToHtmlAsync(QuotationTemplate template, Quotation quotation)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (quotation == null)
                throw new ArgumentNullException(nameof(quotation));
            if (!template.IsFileBased || string.IsNullOrWhiteSpace(template.FileUrl))
                throw new InvalidOperationException("Template is not file-based or file URL is missing");

            _logger.LogInformation("Processing template {TemplateId} to HTML for quotation {QuotationId}", 
                template.TemplateId, quotation.QuotationId);

            // Load quotation with all related data
            var fullQuotation = await _db.Quotations
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .Include(q => q.CreatedByUser)
                .FirstOrDefaultAsync(q => q.QuotationId == quotation.QuotationId);

            if (fullQuotation == null)
                throw new InvalidOperationException($"Quotation {quotation.QuotationId} not found");

            // Get company details
            CompanyDetailsDto? companyDetails = null;
            if (!string.IsNullOrWhiteSpace(fullQuotation.CompanyDetailsSnapshot))
            {
                companyDetails = JsonSerializer.Deserialize<CompanyDetailsDto>(fullQuotation.CompanyDetailsSnapshot);
            }
            if (companyDetails == null)
            {
                companyDetails = await _companyDetailsService.GetCompanyDetailsAsync();
            }

            // Read template file
            var templateBytes = await ReadTemplateFileAsync(template.FileUrl);
            var fileName = template.FileName?.ToLowerInvariant() ?? "";
            var mimeType = template.MimeType?.ToLowerInvariant() ?? "";

            // Extract text from template
            string templateText = "";
            if (mimeType.Contains("html") || fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || 
                fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            {
                templateText = Encoding.UTF8.GetString(templateBytes);
            }
            else if (mimeType.Contains("word") || fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || 
                     fileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            {
                templateText = await ExtractTextFromWordAsync(templateBytes);
            }
            else if (mimeType.Contains("pdf") || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                templateText = await ExtractTextFromPdfAsync(templateBytes);
            }
            else
            {
                _logger.LogWarning("Unsupported template file type for HTML conversion. MimeType: {MimeType}, FileName: {FileName}", mimeType, template.FileName);
                return "<p>Template format not supported for web preview.</p>";
            }

            // Extract intro and terms & conditions
            var (intro, terms) = ExtractIntroAndTerms(templateText);
            
            // Build beautiful HTML with intro and terms sections
            return BuildTemplateSectionsHtml(intro, terms);
        }

        private (string? intro, string? terms) ExtractIntroAndTerms(string templateText)
        {
            if (string.IsNullOrWhiteSpace(templateText))
                return (null, null);

            string? intro = null;
            string? terms = null;

            // Normalize text - replace multiple spaces/newlines with single
            templateText = Regex.Replace(templateText, @"\s+", " ").Trim();

            // Look for intro section - usually at the beginning, before terms
            var introPatterns = new[]
            {
                @"(?i)(?:^|\.)\s*([^.]*(?:simplif|enhanc|innovativ|product|solution|service|company|about|introduction|intro|overview)[^.]*\.(?:\s+[^.]*\.)*)",
                @"(?i)^(.{100,500}?)(?=\s*(?:terms|conditions|payment|delivery|bank|account|inclusion|exclusion))"
            };

            foreach (var pattern in introPatterns)
            {
                var match = Regex.Match(templateText, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    intro = match.Groups[1].Value.Trim();
                    if (intro.Length > 50 && intro.Length < 2000)
                    {
                        break;
                    }
                }
            }

            // Look for Terms & Conditions section
            var termsPatterns = new[]
            {
                @"(?i)(?:terms\s*(?:and|&|&amp;)?\s*conditions?|terms|conditions|payment\s*terms|delivery\s*terms|banking\s*information|account\s*details)\s*[:]?\s*(.+?)(?=\s*(?:thank|regards|sincerely|contact|phone|email|website|$))",
                @"(?i)(?:terms|conditions)\s*[:]?\s*(.+?)(?=\s*(?:thank|regards|sincerely|contact|phone|email|website|$))"
            };

            foreach (var pattern in termsPatterns)
            {
                var match = Regex.Match(templateText, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    terms = match.Groups[1].Value.Trim();
                    // Clean up terms - remove excessive whitespace
                    terms = Regex.Replace(terms, @"\s+", " ").Trim();
                    if (terms.Length > 50 && terms.Length < 5000)
                    {
                        break;
                    }
                }
            }

            // If no specific terms section found, try to get text after common markers
            if (string.IsNullOrWhiteSpace(terms))
            {
                var fallbackPattern = @"(?i)(?:bank|account|ifsc|payment|delivery|inclusion|exclusion|support|training|charges|license|user|admin).{100,2000}";
                var fallbackMatch = Regex.Match(templateText, fallbackPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (fallbackMatch.Success)
                {
                    terms = fallbackMatch.Value.Trim();
                    terms = Regex.Replace(terms, @"\s+", " ").Trim();
                }
            }

            return (intro, terms);
        }

        private string BuildTemplateSectionsHtml(string? intro, string? terms)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class='template-sections space-y-6'>");

            // Intro Section
            if (!string.IsNullOrWhiteSpace(intro))
            {
                html.AppendLine("<div class='intro-section rounded-lg border border-blue-200 bg-blue-50 p-6 shadow-sm dark:border-blue-800 dark:bg-blue-900/20'>");
                html.AppendLine("<h3 class='mb-4 text-lg font-semibold text-blue-900 dark:text-blue-100 flex items-center gap-2'>");
                html.AppendLine("<svg class='h-5 w-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'></path></svg>");
                html.AppendLine("About Us / Introduction");
                html.AppendLine("</h3>");
                html.AppendLine($"<p class='text-blue-800 dark:text-blue-200 leading-relaxed whitespace-pre-wrap'>{System.Net.WebUtility.HtmlEncode(intro)}</p>");
                html.AppendLine("</div>");
            }

            // Terms & Conditions Section
            if (!string.IsNullOrWhiteSpace(terms))
            {
                html.AppendLine("<div class='terms-section rounded-lg border border-amber-200 bg-amber-50 p-6 shadow-sm dark:border-amber-800 dark:bg-amber-900/20'>");
                html.AppendLine("<h3 class='mb-4 text-lg font-semibold text-amber-900 dark:text-amber-100 flex items-center gap-2'>");
                html.AppendLine("<svg class='h-5 w-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'></path></svg>");
                html.AppendLine("Terms & Conditions");
                html.AppendLine("</h3>");
                // Format terms with line breaks and bullet points
                var formattedTerms = FormatTermsText(terms);
                html.AppendLine($"<div class='text-amber-800 dark:text-amber-200 leading-relaxed whitespace-pre-wrap'>{formattedTerms}</div>");
                html.AppendLine("</div>");
            }

            // If no sections found
            if (string.IsNullOrWhiteSpace(intro) && string.IsNullOrWhiteSpace(terms))
            {
                html.AppendLine("<div class='text-center py-8 text-gray-500'>");
                html.AppendLine("<p>No intro or terms & conditions found in template.</p>");
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
            return html.ToString();
        }

        private string FormatTermsText(string terms)
        {
            // Split by common separators and format as list
            var lines = terms.Split(new[] { ". ", ".\n", "\n", "•", "-", "–" }, StringSplitOptions.RemoveEmptyEntries);
            var formatted = new StringBuilder();
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;
                
                // If line looks like a list item or starts with number
                if (Regex.IsMatch(trimmed, @"^(?:\d+[\.\)]|\w+\)|✓|✔|•|\*)\s*", RegexOptions.IgnoreCase))
                {
                    formatted.AppendLine($"• {trimmed}");
                }
                else if (trimmed.Length > 50)
                {
                    // Long paragraph - keep as is
                    formatted.AppendLine(trimmed);
                }
                else
                {
                    // Short line - treat as list item
                    formatted.AppendLine($"• {trimmed}");
                }
            }
            
            return System.Net.WebUtility.HtmlEncode(formatted.ToString().Trim());
        }

        private string ConvertTextToHtml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "<p></p>";

            // Convert plain text to HTML, preserving line breaks and basic formatting
            var html = new StringBuilder();
            html.AppendLine("<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>");
            
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    html.AppendLine("<br/>");
                }
                else
                {
                    // Check if line looks like a heading (all caps, short, or ends with colon)
                    if (trimmedLine.Length < 100 && (trimmedLine == trimmedLine.ToUpper() || trimmedLine.EndsWith(":")))
                    {
                        html.AppendLine($"<h3 style='margin-top: 1em; margin-bottom: 0.5em; font-weight: bold;'>{System.Net.WebUtility.HtmlEncode(trimmedLine)}</h3>");
                    }
                    else
                    {
                        html.AppendLine($"<p style='margin: 0.5em 0;'>{System.Net.WebUtility.HtmlEncode(trimmedLine)}</p>");
                    }
                }
            }
            
            html.AppendLine("</div>");
            return html.ToString();
        }
    }
}

