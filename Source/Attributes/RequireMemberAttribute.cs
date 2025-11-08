using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Source.Models;

namespace Source.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireMemberAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        Member? member = context.HttpContext.Items["Member"] as Member;
        if (member == null)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                success = false,
                message = "User not authenticated"
            });
            return;
        }

        await next();
    }
}
