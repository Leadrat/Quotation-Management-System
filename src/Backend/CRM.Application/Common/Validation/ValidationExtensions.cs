using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace CRM.Application.Common.Validation
{
    public static class ValidationExtensions
    {
        public static Dictionary<string, string[]> ToDictionary(this ValidationResult result)
        {
            return result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }
    }
}
