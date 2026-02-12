using Api.Application.Clients.Contracts;
using Api.Application.Clients.Validators;
using Api.Application.Common.Interfaces;
using Api.Application.Media.Repositories;
using Api.Infrastructure.Background;
using Api.Infrastructure.Config;
using Api.Infrastructure.Mail;
using Api.Infrastructure.Messaging.RabbitMq;
using Api.Infrastructure.Persistence.Cassandra;
using Api.Infrastructure.Persistence.Redis;
using Api.Infrastructure.Persistence.Repositories.Clients;
using Api.Infrastructure.Persistence.Repositories.Media;
using Api.Infrastructure.Security.BCrypt;
using Api.Infrastructure.Security.Jwt;
using Api.Infrastructure.Storage;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace Api.Presentation.Api.DependencyInjection;

public static class ServiceExtensions
{
    // Service Registration
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        // API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"),
                new QueryStringApiVersionReader("api-version")
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssembly(typeof(ClientModelValidator).Assembly);

        // Bcrypt Service
        services.AddScoped<IBCryptService, BCryptService>();

        // JWT Service
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<IJwtService, JwtService>();

        // Redis Service
        services.Configure<RedisSettings>(configuration.GetSection("Redis"));
        services.AddSingleton<IRedisService, RedisService>();

        // Cassandra Service
        services.Configure<CassandraSettings>(configuration.GetSection("Cassandra"));
        services.AddSingleton<ICassandraService, CassandraService>();

        // RabbitMQ Service
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IRabbitMqService, RabbitMqService>();

        // Mail Service
        services.Configure<MailSettings>(configuration.GetSection("Mail"));
        services.AddScoped<IMailService, MailService>();

        // Media Service
        services.Configure<MediaSettings>(configuration.GetSection("Media"));
        services.AddScoped<IMediaService, MediaService>();

        // Workers
        services.AddScoped<IWorkerService, MailService>();
        services.AddHostedService<WorkerService>();

        // Repositories
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();

        // Rate Limiting
        services.Configure<RateLimitSettings>(configuration.GetSection("RateLimit"));
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
        var cassandraService = scope.ServiceProvider.GetRequiredService<ICassandraService>();
        await cassandraService.InitializeKeyspaceAsync();
    }

    public static async Task SeedCassandraDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var cassandraService = scope.ServiceProvider.GetRequiredService<ICassandraService>();
        await cassandraService.SeedDataAsync();
    }

    public static async Task InitializeMediaAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var mediaService = scope.ServiceProvider.GetRequiredService<IMediaService>();
        await mediaService.InitializeBucketAsync();
    }

    // Error Handling
    public static void UseNotFoundHandler(this WebApplication app)
    {
        app.MapFallback(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 404,
                message = "Endpoint not found",
                path = context.Request.Path.Value,
                method = context.Request.Method
            });
        });
    }
}
