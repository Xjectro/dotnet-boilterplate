using Source.Services;
using Source.Extensions;

var configBuilder = new ConfigurationBuilder();
var configuration = configBuilder.Build();

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureKestrelLimits();

builder.Configuration.AddConfiguration(configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddProjectServices(builder.Configuration);

var app = builder.Build();

await app.InitializeCassandraAsync();

app.UseSwaggerDocumentation();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();
