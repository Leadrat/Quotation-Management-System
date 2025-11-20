using CRM.Application.CompanyBankDetails.DTOs;
using FluentValidation;

namespace CRM.Application.CompanyBankDetails.Validators
{
    public class UpdateBankFieldTypeRequestValidator : AbstractValidator<UpdateBankFieldTypeRequest>
    {
        public UpdateBankFieldTypeRequestValidator()
        {
            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}

