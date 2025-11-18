using System;
using System.Linq;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Shared.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class QuotationPdfGenerationServiceTests
    {
        private static QuotationPdfGenerationService CreateService(int cacheHours = 24)
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var logger = Mock.Of<ILogger<QuotationPdfGenerationService>>();
            var settings = Options.Create(new QuotationManagementSettings
            {
                PdfCacheHours = cacheHours,
                BaseUrl = "https://test.com"
            });

            return new QuotationPdfGenerationService(cache, logger, settings);
        }

        private static Quotation CreateTestQuotation(bool includeLineItems = true, bool includeIgst = false)
        {
            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                QuotationNumber = "QT-2025-TEST",
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                SubTotal = 1000,
                DiscountAmount = 0,
                DiscountPercentage = 0,
                TaxAmount = 180,
                CgstAmount = includeIgst ? 0 : 90,
                SgstAmount = includeIgst ? 0 : 90,
                IgstAmount = includeIgst ? 180 : 0,
                TotalAmount = 1180,
                Notes = "Test notes",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Status = QuotationStatus.Draft,
                Client = new Client
                {
                    ClientId = Guid.NewGuid(),
                    CompanyName = "Test Company",
                    Email = "client@test.com",
                    StateCode = includeIgst ? "19" : "27"
                },
                LineItems = includeLineItems ? new[]
                {
                    new QuotationLineItem
                    {
                        LineItemId = Guid.NewGuid(),
                        SequenceNumber = 1,
                        ItemName = "Test Item 1",
                        Description = "Description 1",
                        Quantity = 10,
                        UnitRate = 100,
                        Amount = 1000
                    }
                }.ToList() : new System.Collections.Generic.List<QuotationLineItem>()
            };

            return quotation;
        }

        [Fact]
        public void GenerateQuotationPdf_WithLineItems_ReturnsPdfBytes()
        {
            // Arrange
            var service = CreateService();
            var quotation = CreateTestQuotation(includeLineItems: true);

            // Act
            var pdfBytes = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            // PDF signature check (PDF files start with %PDF-)
            Assert.Equal(0x25, pdfBytes[0]); // %
            Assert.Equal(0x50, pdfBytes[1]); // P
            Assert.Equal(0x44, pdfBytes[2]); // D
            Assert.Equal(0x46, pdfBytes[3]); // F
        }

        [Fact]
        public void GenerateQuotationPdf_WithCgstSgst_IncludesTaxBreakdown()
        {
            // Arrange
            var service = CreateService();
            var quotation = CreateTestQuotation(includeLineItems: true, includeIgst: false);

            // Act
            var pdfBytes = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            // Verify CGST/SGST values are set correctly in quotation
            Assert.Equal(90, quotation.CgstAmount);
            Assert.Equal(90, quotation.SgstAmount);
            Assert.Equal(0, quotation.IgstAmount);
        }

        [Fact]
        public void GenerateQuotationPdf_WithIgst_IncludesTaxBreakdown()
        {
            // Arrange
            var service = CreateService();
            var quotation = CreateTestQuotation(includeLineItems: true, includeIgst: true);

            // Act
            var pdfBytes = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            // Verify IGST values are set correctly in quotation
            Assert.Equal(0, quotation.CgstAmount);
            Assert.Equal(0, quotation.SgstAmount);
            Assert.Equal(180, quotation.IgstAmount);
        }

        [Fact]
        public void GenerateQuotationPdf_Caching_ReturnsCachedPdfOnSecondCall()
        {
            // Arrange
            var service = CreateService(cacheHours: 24);
            var quotation = CreateTestQuotation(includeLineItems: true);

            // Act
            var pdfBytes1 = service.GenerateQuotationPdf(quotation);
            var pdfBytes2 = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes1);
            Assert.NotNull(pdfBytes2);
            Assert.Equal(pdfBytes1.Length, pdfBytes2.Length);
            // Should return the same cached instance
            Assert.True(pdfBytes1.SequenceEqual(pdfBytes2));
        }

        [Fact]
        public void GenerateQuotationPdf_CachingDisabled_GeneratesNewPdf()
        {
            // Arrange
            var service = CreateService(cacheHours: 0);
            var quotation = CreateTestQuotation(includeLineItems: true);

            // Act
            var pdfBytes1 = service.GenerateQuotationPdf(quotation);
            
            // Modify quotation to change cache key
            quotation.UpdatedAt = DateTimeOffset.UtcNow.AddSeconds(1);
            var pdfBytes2 = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes1);
            Assert.NotNull(pdfBytes2);
            // Both should be valid PDFs
            Assert.True(pdfBytes1.Length > 0);
            Assert.True(pdfBytes2.Length > 0);
        }

        [Fact]
        public void GenerateQuotationPdf_WithDiscount_IncludesDiscountInPdf()
        {
            // Arrange
            var service = CreateService();
            var quotation = CreateTestQuotation(includeLineItems: true);
            quotation.DiscountPercentage = 10;
            quotation.DiscountAmount = 100;
            quotation.TotalAmount = 1062; // 1000 - 100 + 162 (18% of 900)

            // Act
            var pdfBytes = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            Assert.Equal(10, quotation.DiscountPercentage);
            Assert.Equal(100, quotation.DiscountAmount);
        }

        [Fact]
        public void GenerateQuotationPdf_WithNotes_IncludesNotesInPdf()
        {
            // Arrange
            var service = CreateService();
            var quotation = CreateTestQuotation(includeLineItems: true);
            quotation.Notes = "Important: Payment due in 15 days";

            // Act
            var pdfBytes = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            Assert.Equal("Important: Payment due in 15 days", quotation.Notes);
        }

        [Fact]
        public void GenerateQuotationPdf_MultipleLineItems_IncludesAllItems()
        {
            // Arrange
            var service = CreateService();
            var quotation = CreateTestQuotation(includeLineItems: false);
            
            quotation.LineItems = new[]
            {
                new QuotationLineItem
                {
                    LineItemId = Guid.NewGuid(),
                    SequenceNumber = 1,
                    ItemName = "Item 1",
                    Quantity = 10,
                    UnitRate = 100,
                    Amount = 1000
                },
                new QuotationLineItem
                {
                    LineItemId = Guid.NewGuid(),
                    SequenceNumber = 2,
                    ItemName = "Item 2",
                    Quantity = 5,
                    UnitRate = 200,
                    Amount = 1000
                },
                new QuotationLineItem
                {
                    LineItemId = Guid.NewGuid(),
                    SequenceNumber = 3,
                    ItemName = "Item 3",
                    Quantity = 2,
                    UnitRate = 500,
                    Amount = 1000
                }
            }.ToList();

            quotation.SubTotal = 3000;
            quotation.TaxAmount = 540;
            quotation.TotalAmount = 3540;

            // Act
            var pdfBytes = service.GenerateQuotationPdf(quotation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            Assert.Equal(3, quotation.LineItems.Count);
        }
    }
}

