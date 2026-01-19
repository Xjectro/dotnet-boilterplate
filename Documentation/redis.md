# Redis Cache Usage

## Overview
Redis is used for high-performance caching and temporary data storage.

## Configuration

### appsettings.json
```json
{
  "Redis": {
    "Host": "redis:6379"
  }
}
```

### Environment Variables (Docker)
```env
Redis__Host=redis:6379
```

## Service Implementation

### RedisService.cs
Location: `Source/Services/RedisService/RedisService.cs`

**Key Features:**
- Connection multiplexing
- Automatic reconnection
- Serialization/deserialization
- TTL (Time To Live) support

### Interface Methods
```csharp
public interface IRedisService
{
    IDatabase GetDatabase();
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}
```

## Usage Examples

### 1. Inject the Service
```csharp
public class ClientController
{
    private readonly IRedisService _redisService;
    public ClientController(IRedisService redisService)
    {
        _redisService = redisService;
    }
}
```

### 2. Set Cache with Expiration
```csharp
// Cache for 5 minutes
await _redisService.SetAsync("user:123", userData, TimeSpan.FromMinutes(5));

// Cache forever (no expiration)
await _redisService.SetAsync("config:app", configData);
```

### 3. Get from Cache
```csharp
var user = await _redisService.GetAsync<User>("user:123");
if (user == null)
{
    // Not in cache, fetch from database
    user = await _database.GetUserAsync(123);
    await _redisService.SetAsync("user:123", user, TimeSpan.FromMinutes(5));
}
return user;
```

### 4. Delete Cache
```csharp
await _redisService.DeleteAsync("user:123");
```

### 5. Check if Key Exists
```csharp
if (await _redisService.ExistsAsync("user:123"))
{
    var user = await _redisService.GetAsync<User>("user:123");
}
```

## Common Use Cases

### 1. Session Storage
```csharp
// Store user session
await _redisService.SetAsync(
    $"session:{sessionId}", 
    sessionData, 
    TimeSpan.FromHours(2)
);

// Retrieve session
var session = await _redisService.GetAsync<SessionData>($"session:{sessionId}");
```

### 2. API Response Caching
```csharp
[HttpGet("products")]
public async Task<IActionResult> GetProducts()
{
    var cacheKey = "products:all";
    
    // Try cache first
    var products = await _redisService.GetAsync<List<Product>>(cacheKey);
    if (products != null)
        return Ok(products);
    
    // Not in cache, fetch from DB
    products = await _database.GetProductsAsync();
    
    // Cache for 10 minutes
    await _redisService.SetAsync(cacheKey, products, TimeSpan.FromMinutes(10));
    
    return Ok(products);
}
```

### 3. Rate Limiting
```csharp
var key = $"ratelimit:{userId}:{DateTime.UtcNow:yyyyMMddHH}";
var count = await _redisService.GetAsync<int>(key);

if (count >= 100)
    return StatusCode(429, "Too many requests");

await _redisService.SetAsync(key, count + 1, TimeSpan.FromHours(1));
```

### 4. Temporary Data
```csharp
// Store verification code
await _redisService.SetAsync(
    $"verify:{email}", 
    verificationCode, 
    TimeSpan.FromMinutes(15)
);

// Check verification
var code = await _redisService.GetAsync<string>($"verify:{email}");
if (code == userInputCode)
{
    await _redisService.DeleteAsync($"verify:{email}");
    // Proceed with verification
}
```

## Cache Key Patterns

Use consistent naming conventions:
```
user:{id}                    → User data
session:{sessionId}          → Session data
products:all                 → All products
product:{id}                 → Single product
ratelimit:{userId}:{hour}    → Rate limiting
verify:{email}               → Verification codes
token:{token}                → Authentication tokens
```

## Data Serialization

Data is automatically serialized to JSON:
```csharp
// Complex objects work automatically
var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
await _redisService.SetAsync("user:1", user);

var retrieved = await _redisService.GetAsync<User>("user:1");
// retrieved is fully deserialized User object
```

## Best Practices

1. **Use Appropriate TTL**: Don't cache forever unless necessary
2. **Key Naming**: Use consistent, hierarchical key names
3. **Invalidation**: Remove stale data when source changes
4. **Size Limits**: Don't cache very large objects
5. **Fallback**: Always have a fallback when cache misses
6. **Monitoring**: Track cache hit/miss rates

## Cache Invalidation Strategies

### 1. Time-Based (TTL)
```csharp
// Automatically expires
await _redisService.SetAsync("data", value, TimeSpan.FromMinutes(10));
```

### 2. Event-Based
```csharp
// When data changes, invalidate cache
public async Task UpdateUser(User user)
{
    await _database.UpdateUserAsync(user);
    await _redisService.DeleteAsync($"user:{user.Id}");
}
```

### 3. Pattern-Based
```csharp
// Delete all user-related caches
var database = _redisService.GetDatabase();
var server = database.Multiplexer.GetServer(endpoint);
await foreach (var key in server.KeysAsync(pattern: "user:*"))
{
    await database.KeyDeleteAsync(key);
}
```

## Performance Tips

1. **Batch Operations**: Use pipelines for multiple operations
2. **Connection Reuse**: Service handles this automatically
3. **Async Operations**: Always use async methods
4. **Small Values**: Redis is optimized for small, frequent operations
5. **Compression**: Consider compressing large values

## Docker Integration

Redis container in `docker-compose.dev.yml`:
- AOF persistence enabled
- Data volume for persistence
- Health checks configured

## Redis CLI

Access Redis CLI in Docker:
```bash
docker exec -it redis redis-cli
```

**Useful Commands:**
```redis
# List all keys
KEYS *

# Get value
GET user:123

# Check TTL (time to live)
TTL user:123

# Delete key
DEL user:123

# Get all keys matching pattern
KEYS user:*

# Flush all data (careful!)
FLUSHALL
```

## Monitoring

Check Redis status:
```bash
# In Redis CLI
INFO

# Memory usage
INFO memory

# Stats
INFO stats

# Connected clients
CLIENT LIST
```

## Error Handling

```csharp
try
{
    await _redisService.SetAsync("key", value);
}
catch (RedisConnectionException ex)
{
    _logger.LogError(ex, "Redis connection failed");
    // Fallback: proceed without cache
}
catch (RedisTimeoutException ex)
{
    _logger.LogError(ex, "Redis operation timeout");
    // Fallback logic
}
```

## Troubleshooting

### Connection Issues
- Verify Redis container is running
- Check network connectivity
- Verify host and port in configuration

### Performance Issues
- Monitor memory usage
- Check for large keys
- Review TTL settings
- Consider Redis maxmemory policy

### Data Not Persisting
- Check AOF/RDB configuration
- Verify volume mounts in Docker
- Review Redis logs
