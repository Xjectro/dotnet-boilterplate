namespace Source.Extensions;

public static class ServiceExtensions
{
    // Service Registration
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bcrypt Service
        services.AddScoped<Source.Services.BCryptService.IBCryptService, Source.Services.BCryptService.BCryptService>();

        // JWT Service
        services.Configure<Source.Configurations.JwtSettings>(configuration.GetSection("JWT_SETTINGS"));
        services.AddSingleton<Source.Services.JwtService.IJwtService, Source.Services.JwtService.JwtService>();

        // Redis Service
        services.Configure<Source.Configurations.RedisSettings>(configuration.GetSection("Redis"));
        services.AddSingleton<Source.Services.RedisService.IRedisService, Source.Services.RedisService.RedisService>();

        // Cassandra Service
        services.Configure<Source.Configurations.CassandraSettings>(configuration.GetSection("Cassandra"));
        services.AddSingleton<Source.Services.CassandraService.ICassandraService, Source.Services.CassandraService.CassandraService>();

        // Repositories
        services.AddScoped<Source.Repositories.ClientRepository.IClientRepository, Source.Repositories.ClientRepository.ClientRepository>();

        return services;
    }

    // Builder Configuration
    public static void ConfigureKestrelLimits(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10);
            options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
            options.AddServerHeader = false;
        });
    }

    // Application Initialization
    public static async Task InitializeCassandraAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var cassandraService = scope.ServiceProvider.GetRequiredService<Source.Services.CassandraService.ICassandraService>();
        await cassandraService.InitializeKeyspaceAsync();
    }
}
