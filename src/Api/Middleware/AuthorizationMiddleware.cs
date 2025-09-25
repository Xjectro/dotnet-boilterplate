using Application.Interfaces;
using Application.Services;
using Domain.Entities;

namespace Api.Middleware;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IJwtService _jwtService;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuthorizationMiddleware(RequestDelegate next, IJwtService jwtService, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _jwtService = jwtService;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _scopeFactory.CreateScope();
        var memberRepository = scope.ServiceProvider.GetRequiredService<IMemberRepository>();

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            var claimsPrincipal = _jwtService.ValidateToken(token);
            if (claimsPrincipal != null)
            {
                var idClaim = claimsPrincipal.FindFirst("id");
                if (idClaim != null && int.TryParse(idClaim.Value, out int memberId))
                {
                    Member? member = await memberRepository.GetByIdAsync(memberId);
                    context.Items["Member"] = member;
                }
            }
        }

        await _next(context);
    }
}
