using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Presentation.Api.Middlewares;

/// <summary>
/// Global filter for model validation
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var response = new
            {
                success = false,
                message = "Validation failed",
                errors = errors,
                statusCode = 400,
                timestamp = DateTime.UtcNow
            };

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
