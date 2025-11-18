using CRM.Application.Quotations.Dtos;
using CRM.Application.Quotations.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.Tests.Quotations
{
    public class CreateLineItemRequestValidatorTests
    {
        private CreateLineItemRequestValidator CreateValidator()
        {
            return new CreateLineItemRequestValidator();
        }

        [Fact]
        public void Validate_ValidLineItem_ShouldPass()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "Test Item",
                Description = "Test Description",
                Quantity = 10,
                UnitRate = 100.50m
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmptyItemName_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "",
                Quantity = 1,
                UnitRate = 100
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemName);
        }

        [Fact]
        public void Validate_ItemNameTooShort_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "A",
                Quantity = 1,
                UnitRate = 100
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ItemName);
        }

        [Fact]
        public void Validate_ZeroQuantity_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "Test Item",
                Quantity = 0,
                UnitRate = 100
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        [Fact]
        public void Validate_NegativeQuantity_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "Test Item",
                Quantity = -1,
                UnitRate = 100
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        [Fact]
        public void Validate_ZeroUnitRate_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "Test Item",
                Quantity = 1,
                UnitRate = 0
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitRate);
        }

        [Fact]
        public void Validate_NegativeUnitRate_ShouldFail()
        {
            // Arrange
            var validator = CreateValidator();
            var request = new CreateLineItemRequest
            {
                ItemName = "Test Item",
                Quantity = 1,
                UnitRate = -1
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitRate);
        }
    }
}

