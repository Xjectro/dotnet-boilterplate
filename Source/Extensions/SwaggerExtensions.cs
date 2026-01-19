using Microsoft.OpenApi.Models;
using System.Reflection;
using Asp.Versioning.ApiExplorer;

namespace Source.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Title = $"API {desc.ApiVersion}",
                    Version = desc.ApiVersion.ToString(),
                    Description = $"A production-ready .NET 10 boilerplate API - Version {desc.ApiVersion}",
                    Contact = new OpenApiContact
                    {
                        Name = "API Support",
                        Email = "xjectro@gmail.com"
                    }
                });
            }

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseReDoc(options =>
        {
            options.RoutePrefix = "redoc";
            options.SpecUrl = "/swagger/v1/swagger.json";
            options.DocumentTitle = "API Redoc Documentation";
        });
        return app;
    }
}
