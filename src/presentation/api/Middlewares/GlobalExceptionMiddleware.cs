using Api.Application.Common;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;
using System.Text.Json;

namespace Api.Presentation.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred";

        // Specific exception handling (more specific first)
        switch (exception)
        {
            case ArgumentNullException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Required parameter is missing";
                break;
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Invalid request parameters";
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access";
                break;
            case KeyNotFoundException:
            case FileNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "Resource not found";
                break;
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Invalid operation";
                break;
        }

        var errorList = new List<string> { exception.Message };
        var apiResponse = new ApiResponse<object>(
            message: message,
            errors: errorList,
            success: false
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse, options));
    }
}
