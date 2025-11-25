using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;
using Source.Models;
using Source.Repositories;
using Source.Services;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var configBuilder = new ConfigurationBuilder();
var configuration = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfiguration(configuration);

builder.Configuration
    .AddYamlFile("Configurations/appsettings.yml")
    .AddYamlFile($"Configurations/appsettings.{builder.Environment.EnvironmentName}.yml", optional: true);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var connectionString = builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"];
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddScoped<IMemberRepository>(provider => new MemberRepository(connectionString));
builder.Services.AddScoped<IBCryptService, BCryptService>();
builder.Services.AddSingleton<IJwtService, JwtService>();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT token giriniz: Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.MapSwagger("/openapi/{documentName}.json");
app.MapScalarApiReference("/docs");

var urls = Environment.GetEnvironmentVariable("DOTNET_URLS");
if (!string.IsNullOrEmpty(urls))
{
    app.Urls.Clear();
    app.Urls.Add(urls);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseMiddleware<Source.Middleware.AuthorizationMiddleware>();
app.UseMiddleware<Source.Middleware.ResponseTimeMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
