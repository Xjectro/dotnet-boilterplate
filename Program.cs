using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;
using Source.Models;
using Source.Repositories;
using Source.Services;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

Dictionary<string, object> LoadYamlConfig(string environment)
{
    var yamlFile = environment == "Production"
        ? "Configurations/appsettings.Production.yml"
        : "Configurations/appsettings.yml";
    var yamlText = File.ReadAllText(yamlFile);
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    return deserializer.Deserialize<Dictionary<string, object>>(yamlText);
}

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var yamlConfig = LoadYamlConfig(env);

var configBuilder = new ConfigurationBuilder();
configBuilder.AddInMemoryCollection(
    yamlConfig.Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value?.ToString()))
);
var configuration = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddConfiguration(configuration);


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
