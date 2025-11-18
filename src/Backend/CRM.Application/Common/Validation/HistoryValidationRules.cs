using FluentValidation;

namespace CRM.Application.Common.Validation
{
    public static class HistoryValidationRules
    {
        public static IRuleBuilderOptions<T, int> HistoryPageNumber<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.GreaterThanOrEqualTo(1)
                              .WithMessage("pageNumber must be greater than or equal to 1.");
        }

        public static IRuleBuilderOptions<T, int> HistoryPageSize<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.InclusiveBetween(1, 100)
                              .WithMessage("pageSize must be between 1 and 100.");
        }

        public static IRuleBuilderOptions<T, string?> RestoreReason<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.NotEmpty()
                              .MinimumLength(5)
                              .MaximumLength(500)
                              .WithMessage("Reason must be between 5 and 500 characters.");
        }

        public static IRuleBuilderOptions<T, int> ExportRowLimit<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.LessThanOrEqualTo(5000)
                              .WithMessage("Export exceeds the maximum allowed rows of 5000.");
        }
    }
}

