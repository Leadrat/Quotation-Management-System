using System;
using FluentValidation;

namespace CRM.Application.Clients.Validators
{
    public class SearchClientsQueryValidator : AbstractValidator<Clients.Queries.SearchClientsQuery>
    {
        public SearchClientsQueryValidator()
        {
            RuleFor(q => q.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0");
            RuleFor(q => q.PageSize).InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
            RuleFor(q => q.SearchTerm).MaximumLength(255).When(q => !string.IsNullOrEmpty(q.SearchTerm))
                .WithMessage("Search term must not exceed 255 characters");
            RuleFor(q => q).Must(q => !q.CreatedDateFrom.HasValue || !q.CreatedDateTo.HasValue || q.CreatedDateFrom <= q.CreatedDateTo)
                .WithMessage("CreatedDateFrom must be before CreatedDateTo");
            RuleFor(q => q).Must(q => !q.UpdatedDateFrom.HasValue || !q.UpdatedDateTo.HasValue || q.UpdatedDateFrom <= q.UpdatedDateTo)
                .WithMessage("UpdatedDateFrom must be before UpdatedDateTo");
        }
    }
}
