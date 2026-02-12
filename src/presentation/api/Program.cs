using Api.Presentation.Api.DependencyInjection;
using Api.Presentation.Api.Middlewares;
using Serilog;

Log.Information("Starting web application");

var configBuilder = new ConfigurationBuilder();
var configuration = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();
builder.ConfigureKestrelLimits();

builder.Configuration.AddConfiguration(configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

await app.InitializeCassandraAsync();
await app.SeedCassandraDataAsync();
await app.InitializeMediaAsync();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwaggerDocumentation();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.UseNotFoundHandler();

app.Run();
