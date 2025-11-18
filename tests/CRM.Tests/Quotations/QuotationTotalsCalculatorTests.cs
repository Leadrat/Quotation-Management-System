using System.Collections.Generic;
using CRM.Application.Quotations.Services;
using CRM.Domain.Entities;
using CRM.Shared.Config;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class QuotationTotalsCalculatorTests
    {
        private QuotationTotalsCalculator CreateCalculator()
        {
            return new QuotationTotalsCalculator();
        }

        [Fact]
        public void Calculate_WithLineItems_ReturnsCorrectSubtotal()
        {
            // Arrange
            var calculator = CreateCalculator();
            var quotation = new Quotation();
            var lineItems = new List<QuotationLineItem>
            {
                new QuotationLineItem { Quantity = 10, UnitRate = 1000, Amount = 10000 },
                new QuotationLineItem { Quantity = 5, UnitRate = 500, Amount = 2500 }
            };
            var discountPercentage = 0m;

            // Act
            var result = calculator.Calculate(quotation, lineItems, discountPercentage);

            // Assert
            Assert.Equal(12500m, result.SubTotal);
            Assert.Equal(0m, result.DiscountAmount);
        }

        [Fact]
        public void Calculate_WithDiscount_ReturnsCorrectDiscountAmount()
        {
            // Arrange
            var calculator = CreateCalculator();
            var quotation = new Quotation();
            var lineItems = new List<QuotationLineItem>
            {
                new QuotationLineItem { Quantity = 10, UnitRate = 1000, Amount = 10000 }
            };
            var discountPercentage = 10m;

            // Act
            var result = calculator.Calculate(quotation, lineItems, discountPercentage);

            // Assert
            Assert.Equal(10000m, result.SubTotal);
            Assert.Equal(1000m, result.DiscountAmount);
            Assert.Equal(10m, result.DiscountPercentage);
        }

        [Fact]
        public void Calculate_DiscountCannotExceedSubtotal()
        {
            // Arrange
            var calculator = CreateCalculator();
            var quotation = new Quotation();
            var lineItems = new List<QuotationLineItem>
            {
                new QuotationLineItem { Quantity = 1, UnitRate = 100, Amount = 100 }
            };
            var discountPercentage = 150m; // More than 100%

            // Act
            var result = calculator.Calculate(quotation, lineItems, discountPercentage);

            // Assert - Discount should be capped at subtotal
            Assert.Equal(100m, result.SubTotal);
            Assert.True(result.DiscountAmount <= result.SubTotal);
        }

        [Fact]
        public void Calculate_WithMultipleLineItems_CalculatesCorrectly()
        {
            // Arrange
            var calculator = CreateCalculator();
            var quotation = new Quotation();
            var lineItems = new List<QuotationLineItem>
            {
                new QuotationLineItem { Quantity = 2, UnitRate = 50, Amount = 100 },
                new QuotationLineItem { Quantity = 3, UnitRate = 75, Amount = 225 },
                new QuotationLineItem { Quantity = 1, UnitRate = 200, Amount = 200 }
            };
            var discountPercentage = 5m;

            // Act
            var result = calculator.Calculate(quotation, lineItems, discountPercentage);

            // Assert
            Assert.Equal(525m, result.SubTotal);
            Assert.Equal(26.25m, result.DiscountAmount, 2);
        }
    }
}

