using System;
using CRM.Application.Quotations.Services;
using CRM.Shared.Config;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class TaxCalculationServiceTests
    {
        private TaxCalculationService CreateService(string companyStateCode = "27")
        {
            var companySettings = Options.Create(new CompanySettings
            {
                StateCode = companyStateCode,
                StateName = "Maharashtra"
            });
            return new TaxCalculationService(companySettings);
        }

        [Fact]
        public void CalculateTax_IntraState_ReturnsCGSTAndSGST()
        {
            // Arrange
            var service = CreateService("27");
            var subtotal = 10000m;
            var discountAmount = 1000m;
            var clientStateCode = "27"; // Same as company state

            // Act
            var result = service.CalculateTax(subtotal, discountAmount, clientStateCode);

            // Assert
            Assert.Equal(0, result.IgstAmount);
            Assert.True(result.CgstAmount > 0);
            Assert.True(result.SgstAmount > 0);
            Assert.Equal(result.CgstAmount, result.SgstAmount); // CGST and SGST should be equal
            Assert.Equal(result.CgstAmount + result.SgstAmount, result.TotalTax);
        }

        [Fact]
        public void CalculateTax_InterState_ReturnsIGST()
        {
            // Arrange
            var service = CreateService("27");
            var subtotal = 10000m;
            var discountAmount = 1000m;
            var clientStateCode = "06"; // Different state (Delhi)

            // Act
            var result = service.CalculateTax(subtotal, discountAmount, clientStateCode);

            // Assert
            Assert.Equal(0, result.CgstAmount);
            Assert.Equal(0, result.SgstAmount);
            Assert.True(result.IgstAmount > 0);
            Assert.Equal(result.IgstAmount, result.TotalTax);
        }

        [Fact]
        public void CalculateTax_NoStateCode_DefaultsToInterState()
        {
            // Arrange
            var service = CreateService("27");
            var subtotal = 10000m;
            var discountAmount = 1000m;

            // Act
            var result = service.CalculateTax(subtotal, discountAmount, null);

            // Assert
            Assert.Equal(0, result.CgstAmount);
            Assert.Equal(0, result.SgstAmount);
            Assert.True(result.IgstAmount > 0);
        }

        [Fact]
        public void CalculateTax_CorrectTaxAmounts_IntraState()
        {
            // Arrange
            var service = CreateService("27");
            var subtotal = 10000m;
            var discountAmount = 1000m;
            var taxableAmount = subtotal - discountAmount; // 9000
            var clientStateCode = "27";

            // Act
            var result = service.CalculateTax(subtotal, discountAmount, clientStateCode);

            // Assert
            // 9% CGST on 9000 = 405
            // 9% SGST on 9000 = 405
            var expectedCgst = taxableAmount * 0.09m;
            var expectedSgst = taxableAmount * 0.09m;
            Assert.Equal(expectedCgst, result.CgstAmount, 2);
            Assert.Equal(expectedSgst, result.SgstAmount, 2);
            Assert.Equal(expectedCgst + expectedSgst, result.TotalTax, 2);
        }

        [Fact]
        public void CalculateTax_CorrectTaxAmounts_InterState()
        {
            // Arrange
            var service = CreateService("27");
            var subtotal = 10000m;
            var discountAmount = 1000m;
            var taxableAmount = subtotal - discountAmount; // 9000
            var clientStateCode = "06";

            // Act
            var result = service.CalculateTax(subtotal, discountAmount, clientStateCode);

            // Assert
            // 18% IGST on 9000 = 1620
            var expectedIgst = taxableAmount * 0.18m;
            Assert.Equal(expectedIgst, result.IgstAmount, 2);
            Assert.Equal(expectedIgst, result.TotalTax, 2);
        }

        [Fact]
        public void CalculateTax_CaseInsensitiveStateCode()
        {
            // Arrange
            var service = CreateService("27");
            var subtotal = 10000m;
            var discountAmount = 1000m;

            // Act
            var result1 = service.CalculateTax(subtotal, discountAmount, "27");
            var result2 = service.CalculateTax(subtotal, discountAmount, " 27 ");
            var result3 = service.CalculateTax(subtotal, discountAmount, "27");

            // Assert - All should be treated as intra-state
            Assert.True(result1.CgstAmount > 0);
            Assert.True(result2.CgstAmount > 0);
            Assert.True(result3.CgstAmount > 0);
        }
    }
}

