using System;
using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class CreateQuotationRequestValidatorTests
    {
        private CreateQuotationRequestValidator CreateValidator()
        {
            return new CreateQuotationRequestValidator();
        }

        [Fact]
        public void Validate_ValidRequest_ShouldPass()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateQuotationRequest
            {
                ClientId = Guid.NewGuid(),
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(30),
                DiscountPercentage = 10,
                LineItems = new List<CreateLineItemRequest>
                {
                    new CreateLineItemRequest
                    {
                        ItemName = "Test Item",
                        Quantity = 1,
                        UnitRate = 100
                    }
                }
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmptyClientId_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateQuotationRequest
            {
                ClientId = Guid.Empty,
                LineItems = new List<CreateLineItemRequest>
                {
                    new CreateLineItemRequest { ItemName = "Test", Quantity = 1, UnitRate = 100 }
                }
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ClientId);
        }

        [Fact]
        public void Validate_FutureQuotationDate_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateQuotationRequest
            {
                ClientId = Guid.NewGuid(),
                QuotationDate = DateTime.Today.AddDays(1),
                LineItems = new List<CreateLineItemRequest>
                {
                    new CreateLineItemRequest { ItemName = "Test", Quantity = 1, UnitRate = 100 }
                }
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.QuotationDate);
        }

        [Fact]
        public void Validate_ValidUntilBeforeQuotationDate_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateQuotationRequest
            {
                ClientId = Guid.NewGuid(),
                QuotationDate = DateTime.Today,
                ValidUntil = DateTime.Today.AddDays(-1),
                LineItems = new List<CreateLineItemRequest>
                {
                    new CreateLineItemRequest { ItemName = "Test", Quantity = 1, UnitRate = 100 }
                }
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ValidUntil);
        }

        [Fact]
        public void Validate_DiscountPercentageOver100_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateQuotationRequest
            {
                ClientId = Guid.NewGuid(),
                DiscountPercentage = 101,
                LineItems = new List<CreateLineItemRequest>
                {
                    new CreateLineItemRequest { ItemName = "Test", Quantity = 1, UnitRate = 100 }
                }
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage);
        }

        [Fact]
        public void Validate_NoLineItems_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateQuotationRequest
            {
                ClientId = Guid.NewGuid(),
                LineItems = new List<CreateLineItemRequest>()
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.LineItems);
        }
    }
}

