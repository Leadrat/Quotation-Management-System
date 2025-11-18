using FluentValidation;
using CRM.Application.Clients.Queries;

namespace CRM.Application.Clients.Validation
{
    public class GetClientTimelineQueryValidator : AbstractValidator<GetClientTimelineQuery>
    {
        public GetClientTimelineQueryValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.RequestorUserId).NotEmpty();
        }
    }
}

