using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Domain.Entities;
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

        public QuotationPdfGenerationService(
            IMemoryCache cache,
            ILogger<QuotationPdfGenerationService> logger,
            IOptions<QuotationManagementSettings> settings)
        {
            _cache = cache;
            _logger = logger;
            _settings = settings.Value;
        }

        public byte[] GenerateQuotationPdf(Quotation quotation)
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

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => Header(c, quotation));
                    page.Content().Element(c => Content(c, quotation));
                    page.Footer().Element(c => Footer(c, quotation));
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

        private static void Header(IContainer container, Quotation quotation)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Column(column =>
                {
                    column.Item().Text("QUOTATION").FontSize(20).Bold().FontColor(Colors.Green.Darken3);
                    column.Item().Text($"Quotation #{quotation.QuotationNumber}").FontSize(14);
                });

                row.ConstantColumn(100).Column(column =>
                {
                    column.Item().AlignRight().Text("Date: " + quotation.QuotationDate.ToString("dd MMM yyyy"));
                    column.Item().AlignRight().Text("Valid Until: " + quotation.ValidUntil.ToString("dd MMM yyyy"));
                });
            });
        }

        private static void Content(IContainer container, Quotation quotation)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Client Information
                column.Item().PaddingBottom(10).Column(clientColumn =>
                {
                    clientColumn.Item().Text("Bill To:").FontSize(12).Bold();
                    clientColumn.Item().Text(quotation.Client?.CompanyName ?? "N/A");
                    if (quotation.Client != null)
                    {
                        clientColumn.Item().Text(quotation.Client.Email ?? "");
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
                        row.RelativeColumn();
                        row.ConstantColumn(200).Column(col =>
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeColumn().Text("Subtotal:");
                                r.ConstantColumn(100).AlignRight().Text(quotation.SubTotal.ToString("N2"));
                            });
                            if (quotation.DiscountAmount > 0)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeColumn().Text($"Discount ({quotation.DiscountPercentage}%):");
                                    r.ConstantColumn(100).AlignRight().Text($"-{quotation.DiscountAmount.ToString("N2")}");
                                });
                            }
                            if (quotation.CgstAmount > 0)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeColumn().Text("CGST (9%):");
                                    r.ConstantColumn(100).AlignRight().Text(quotation.CgstAmount.Value.ToString("N2"));
                                });
                                col.Item().Row(r =>
                                {
                                    r.RelativeColumn().Text("SGST (9%):");
                                    r.ConstantColumn(100).AlignRight().Text(quotation.SgstAmount.Value.ToString("N2"));
                                });
                            }
                            if (quotation.IgstAmount > 0)
                            {
                                col.Item().Row(r =>
                                {
                                    r.RelativeColumn().Text("IGST (18%):");
                                    r.ConstantColumn(100).AlignRight().Text(quotation.IgstAmount.Value.ToString("N2"));
                                });
                            }
                            col.Item().PaddingTop(5).BorderTop(1).Row(r =>
                            {
                                r.RelativeColumn().Text("Total Amount:").Bold().FontSize(12);
                                r.ConstantColumn(100).AlignRight().Text(quotation.TotalAmount.ToString("N2")).Bold().FontSize(12);
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
            });
        }

        private static void Footer(IContainer container, Quotation quotation)
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
    }
}
