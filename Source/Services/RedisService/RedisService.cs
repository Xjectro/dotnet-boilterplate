using Source.Configurations;
using StackExchange.Redis;

namespace Source.Services.RedisService;

public class RedisService : IRedisService
{
    private readonly RedisSettings _settings;
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection;

    public RedisService(Microsoft.Extensions.Options.IOptions<RedisSettings> options)
    {
        _settings = options.Value;
        _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_settings.Host));
    }

    private ConnectionMultiplexer Connection => _lazyConnection.Value;

    public async Task SetAsync(string key, string value)
    {
        var db = Connection.GetDatabase();
        await db.StringSetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        var db = Connection.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }
}
