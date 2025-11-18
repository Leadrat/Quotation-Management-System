using CRM.Application.Clients.Commands;
using CRM.Application.Common.Validation;
using FluentValidation;

namespace CRM.Application.Clients.Validators
{
    public class RestoreClientCommandValidator : AbstractValidator<RestoreClientCommand>
    {
        public RestoreClientCommandValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.RequestorUserId).NotEmpty();
            RuleFor(x => x.RequestorRole).NotEmpty();
            RuleFor(x => x.Reason).RestoreReason();
        }
    }
}

