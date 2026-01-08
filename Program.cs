using Source.Services;
using Source.Extensions;

var configBuilder = new ConfigurationBuilder();
var configuration = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelLimits();

builder.Configuration.AddConfiguration(configuration);

builder.Configuration
    .AddJsonFile($"Configurations/appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"Configurations/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

app.UseSwaggerDocumentation();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
