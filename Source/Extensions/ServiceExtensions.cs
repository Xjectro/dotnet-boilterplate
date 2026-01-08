namespace Source.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bcrypt Service
        services.AddScoped<Source.Services.BCryptService.IBCryptService, Source.Services.BCryptService.BCryptService>();

        // JWT Service
        services.Configure<Source.Configurations.JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<Source.Services.JwtService.IJwtService, Source.Services.JwtService.JwtService>();

        // Redis Service
        services.Configure<Source.Configurations.RedisSettings>(configuration.GetSection("Redis"));
        services.AddSingleton<Source.Services.RedisService.IRedisService, Source.Services.RedisService.RedisService>();

        return services;
    }

    public static void ConfigureKestrelLimits(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
            options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
            options.AddServerHeader = false;
        });
    }
}
