using FluentValidation;
using CRM.Application.Clients.Queries;

namespace CRM.Application.Clients.Validation
{
    public class GetClientSearchSuggestionsQueryValidator : AbstractValidator<GetClientSearchSuggestionsQuery>
    {
        public GetClientSearchSuggestionsQueryValidator()
        {
            RuleFor(q => q.SearchTerm)
                .NotEmpty().WithMessage("Search term is required")
                .MinimumLength(2).WithMessage("Search term must be at least 2 characters")
                .MaximumLength(255).WithMessage("Search term must not exceed 255 characters");
            RuleFor(q => q.MaxSuggestions)
                .InclusiveBetween(1, 50).WithMessage("Max suggestions must be between 1 and 50");
        }
    }
}
