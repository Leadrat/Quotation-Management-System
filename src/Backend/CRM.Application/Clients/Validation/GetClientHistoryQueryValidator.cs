using FluentValidation;
using CRM.Application.Clients.Queries;
using CRM.Application.Common.Validation;

namespace CRM.Application.Clients.Validation
{
    public class GetClientHistoryQueryValidator : AbstractValidator<GetClientHistoryQuery>
    {
        public GetClientHistoryQueryValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.RequestorUserId).NotEmpty();
            RuleFor(x => x.PageNumber).HistoryPageNumber();
            RuleFor(x => x.PageSize).HistoryPageSize();
        }
    }
}

