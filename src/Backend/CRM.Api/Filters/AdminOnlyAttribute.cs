using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CRM.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        
        // Check if user is authenticated
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new StatusCodeResult(401);
            return;
        }

        // Check if user has Admin role
        // JWT should have "role" claim with value "Admin"
        var roleClaim = user.Claims.FirstOrDefault(c => 
            c.Type == "role" || 
            c.Type == ClaimTypes.Role || 
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        
        var isAdmin = roleClaim != null && 
                     string.Equals(roleClaim.Value, "Admin", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            context.Result = new StatusCodeResult(403);
            return;
        }

        await next();
    }
}
