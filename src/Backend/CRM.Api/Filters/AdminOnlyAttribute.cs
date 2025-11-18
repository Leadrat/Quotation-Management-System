using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Placeholder authorization: require header X-Admin: true
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Admin", out var values) ||
            !string.Equals(values.ToString(), "true", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new StatusCodeResult(403);
            return;
        }
        await next();
    }
}
