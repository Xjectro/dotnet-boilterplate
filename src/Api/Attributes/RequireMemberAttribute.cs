using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Domain.Entities;

namespace Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireMemberAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Items.TryGetValue("Member", out var memberObj) || memberObj is not Member)
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
