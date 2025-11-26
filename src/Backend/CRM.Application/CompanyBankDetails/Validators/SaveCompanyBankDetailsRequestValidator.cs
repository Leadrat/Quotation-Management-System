using CRM.Application.CompanyBankDetails.DTOs;
using CRM.Application.CompanyBankDetails.Services;
using FluentValidation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Application.CompanyBankDetails.Validators
{
    public class SaveCompanyBankDetailsRequestValidator : AbstractValidator<SaveCompanyBankDetailsRequest>
    {
        private readonly ICompanyBankDetailsValidationService _validationService;

        public SaveCompanyBankDetailsRequestValidator(ICompanyBankDetailsValidationService validationService)
        {
            _validationService = validationService;

            RuleFor(x => x.CountryId)
                .NotEmpty();

            // Custom validation using the validation service
            RuleFor(x => x)
                .MustAsync(ValidateAgainstConfiguration)
                .WithMessage("Validation failed. Check field errors for details.");
        }

        private async Task<bool> ValidateAgainstConfiguration(SaveCompanyBankDetailsRequest request, CancellationToken cancellationToken)
        {
            var errors = await _validationService.ValidateAsync(request.CountryId, request.Values);
            return !errors.Any();
        }
    }
}

