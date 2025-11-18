using FluentValidation;
using CRM.Application.Clients.Queries;
using CRM.Application.Common.Validation;

namespace CRM.Application.Clients.Validation
{
    public class GetUserActivityQueryValidator : AbstractValidator<GetUserActivityQuery>
    {
        public GetUserActivityQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.RequestorUserId).NotEmpty();
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
            {
                RuleFor(x => x.DateTo).GreaterThanOrEqualTo(x => x.DateFrom)
                    .WithMessage("DateTo must be greater than or equal to DateFrom");
            });
        }
    }
}

