using CRM.Application.CompanyIdentifiers.DTOs;
using CRM.Application.CompanyIdentifiers.Services;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.CompanyIdentifiers.Validators
{
    public class SaveCompanyIdentifierValuesRequestValidator : AbstractValidator<SaveCompanyIdentifierValuesRequest>
    {
        private readonly ICompanyIdentifierValidationService _validationService;

        public SaveCompanyIdentifierValuesRequestValidator(ICompanyIdentifierValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(x => x.CountryId)
                .NotEmpty();

            // Custom validation using the validation service
            RuleFor(x => x)
                .MustAsync(ValidateAgainstConfiguration)
                .WithMessage("Validation failed. Check field errors for details.");
        }

        private async Task<bool> ValidateAgainstConfiguration(SaveCompanyIdentifierValuesRequest request, CancellationToken cancellationToken)
        {
            var errors = await _validationService.ValidateAsync(request.CountryId, request.Values);
            
            // Store errors in custom state for later retrieval
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    var identifierTypeId = error.Key;
                    foreach (var message in error.Value)
                    {
                        // Add custom validation error
                        var propertyName = $"Values[{identifierTypeId}]";
                        // Note: FluentValidation doesn't support dynamic property names directly
                        // We'll rely on the service to throw a domain validation exception instead
                    }
                }
                return false;
            }

            return true;
        }
    }
}

