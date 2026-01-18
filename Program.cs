using Source.Services;
using Source.Extensions;
using Serilog;

try
{
    Log.Information("Starting web application");

    var configBuilder = new ConfigurationBuilder();
    var configuration = configBuilder.Build();

    var builder = WebApplication.CreateBuilder(args);

    builder.ConfigureSerilog();
    builder.ConfigureKestrelLimits();

    builder.Configuration.AddConfiguration(configuration);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddProjectServices(builder.Configuration);

    var app = builder.Build();

    await app.InitializeCassandraAsync();
    await app.InitializeMediaAsync();

    app.UseSerilogRequestLogging();
    app.UseMiddleware<Source.Middlewares.GlobalExceptionMiddleware>();
    app.UseSwaggerDocumentation();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthorization();
    app.MapControllers();
    app.UseNotFoundHandler();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
