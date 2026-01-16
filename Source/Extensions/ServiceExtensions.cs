using System.Threading.RateLimiting;

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

        // RabbitMQ Service
        services.Configure<Source.Configurations.RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<Source.Services.RabbitMqService.IRabbitMqService, Source.Services.RabbitMqService.RabbitMqService>();

        // Mail Service
        services.Configure<Source.Configurations.MailSettings>(configuration.GetSection("Mail"));
        services.AddScoped<Source.Services.MailService.IMailService, Source.Services.MailService.MailService>();
        
        // Workers
        services.AddScoped<Source.Services.WorkerService.IWorkerService, Source.Services.MailService.MailService>();
        services.AddHostedService<Source.Services.WorkerService.WorkerService>();

        // Repositories
        services.AddScoped<Source.Repositories.ClientRepository.IClientRepository, Source.Repositories.ClientRepository.ClientRepository>();

        // Rate Limiting
        services.Configure<Source.Configurations.RateLimitSettings>(configuration.GetSection("RateLimit"));
        services.AddRateLimiter(options =>
        {
            // Default policy - 100 requests per minute
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Strict policy - 10 requests per minute (for sensitive endpoints)
            options.AddPolicy("strict", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Token bucket policy - burst support with 50 tokens, 10 per minute refill
            options.AddPolicy("token", httpContext =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 50,
                        QueueLimit = 0,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        TokensPerPeriod = 10,
                        AutoReplenishment = true
                    }));

            // Sliding window policy - 30 requests per minute with 6 segments
            options.AddPolicy("sliding", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: partition => new SlidingWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 30,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6
                    }));

            // Custom 429 response
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.", token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.", token);
                }
            };
        });

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
