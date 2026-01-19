# Rate Limiting

This document explains the request limiting implementation in the API using .NET's built-in rate limiting middleware.

## Overview

The API uses .NET 7+ rate limiting to prevent abuse and DDoS attacks. Different strategies and policies support various use cases.

## Configuration

### Rate Limit Settings

Configured via `appsettings.json`:

```json
{
    "RateLimit": {
        "PermitLimit": 100,
        "WindowSeconds": 60,
        "QueueLimit": 0
    }
}
```

### Environment Variables (Docker)

```yaml
environment:
    - RateLimit__PermitLimit=100
    - RateLimit__WindowSeconds=60
    - RateLimit__QueueLimit=0
```

## Rate Limiting Policies

### 1. Global Policy (Default)

- Strategy: Fixed Window
- Limit: 100 requests per minute per IP
- Queue: 0 (immediate reject)

Applied to all endpoints by default.

```csharp
// Automatically applied to all endpoints
```

### 2. Strict Policy

- Strategy: Fixed Window
- Limit: 10 requests per minute per IP
- Queue: 0 (immediate reject)

Used for sensitive endpoints like authentication, mail sending, etc.

```csharp
[EnableRateLimiting("strict")]
[HttpPost("send")]
public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
{
        // Only 10 requests per minute allowed
}
```

### 3. Token Bucket Policy

- Strategy: Token Bucket
- Token Limit: 50
- Refill Rate: 10 tokens per minute
- Queue: 0 (immediate reject)

Allows up to 50 burst requests, then throttles to 10 per minute. Good for variable load APIs.

```csharp
[EnableRateLimiting("token")]
[HttpGet("search")]
public async Task<IActionResult> Search([FromQuery] string query)
{
        // Supports bursts up to 50 requests
}
```

### 4. Sliding Window Policy

- Strategy: Sliding Window
- Limit: 30 requests per minute per IP
- Segments: 6 (10-second windows)
- Queue: 0 (immediate reject)

More accurate than fixed window, prevents burst attacks. Good for critical endpoints.

```csharp
[EnableRateLimiting("sliding")]
[HttpPost("payment")]
public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
{
        // Smoothly distributed 30 requests per minute
}
```

## Rate Limiting Strategies Comparison

| Strategy | Best For | Burst Support | Accuracy | Memory Usage |
|----------|----------|---------------|----------|--------------|
| Fixed Window | General APIs | Low | Medium | Low |
| Token Bucket | Variable load | High | Medium | Medium |
| Sliding Window | Critical endpoints | Low | High | High |
| Concurrency | Active connections | N/A | Real-time | Low |

## Usage Examples

### Disable Rate Limiting for Specific Endpoint

```csharp
[DisableRateLimiting]
[HttpGet("health")]
public IActionResult Health()
{
    return Ok("Healthy");
}
```

### Controller-Level Rate Limiting

```csharp
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("sliding")]
public class PaymentController : ControllerBase
{
    // All endpoints use sliding window policy
}
```

### Override Controller Policy

```csharp
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("sliding")]
public class PaymentController : ControllerBase
{
    [EnableRateLimiting("token")]
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        // Uses token bucket instead of sliding
    }
}
```

## Response Headers

Rate limit information is included in response headers:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1234567890
Retry-After: 42
```

## 429 Too Many Requests Response

When rate limit is exceeded:

```json
{
  "status": 429,
  "message": "Too many requests. Please try again after 42 seconds."
}
```

## Partition Keys

Rate limits are applied per partition key. By default, the partition key is the client's IP address:

```csharp
partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous"
```

### Custom Partition Keys

You can customize partition keys for user-based or API key-based rate limiting:

```csharp
// Rate limit by user ID
options.AddPolicy("user-based", httpContext =>
{
    var userId = httpContext.User.FindFirst("sub")?.Value ?? "anonymous";
    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: userId,
        factory: partition => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1)
        });
});
```

## Best Practices

### 1. Choose Appropriate Strategy

- **Fixed Window**: Simple, low memory, good for most APIs
- **Token Bucket**: Burst-friendly, good for user-facing APIs
- **Sliding Window**: Accurate, good for sensitive operations
- **Concurrency**: Limit active connections, good for long-running operations

### 2. Set Reasonable Limits

```csharp
// Too restrictive - bad UX
PermitLimit = 5,  // Only 5 requests per minute

// Too permissive - vulnerable to abuse
PermitLimit = 10000,  // 10k requests per minute

// Balanced
PermitLimit = 100,  // 100 requests per minute
```

### 3. Use Different Policies for Different Endpoints

```csharp
[EnableRateLimiting("strict")]   // 10/min - Auth endpoints
[EnableRateLimiting("token")]    // 50 burst - Search/Read
[EnableRateLimiting("sliding")]  // 30/min - Write operations
```

### 4. Monitor Rate Limit Hits

```csharp
options.OnRejected = async (context, token) =>
{
    _logger.LogWarning($"Rate limit exceeded for {context.HttpContext.Connection.RemoteIpAddress}");
    // Implement monitoring/alerting
};
```

### 5. Consider Authenticated Users

```csharp
// Higher limits for authenticated users
var userId = httpContext.User.Identity?.IsAuthenticated == true 
    ? httpContext.User.FindFirst("sub")?.Value 
    : httpContext.Connection.RemoteIpAddress?.ToString();

var permitLimit = httpContext.User.Identity?.IsAuthenticated == true ? 200 : 50;
```

## Distributed Rate Limiting

For multi-instance deployments, consider using Redis for distributed rate limiting:

```csharp
// Using StackExchange.Redis.RateLimiting (future enhancement)
services.AddStackExchangeRedisRateLimiting(options =>
{
    options.ConfigurationOptions = ConfigurationOptions.Parse("localhost:6379");
});
```

## Testing Rate Limits

### Using curl

```bash
# Test fixed window (100 requests/min)
for i in {1..105}; do
  curl -X GET http://localhost:5000/api/health
  echo "Request $i"
done

# Test strict policy (10 requests/min)
for i in {1..15}; do
  curl -X POST http://localhost:5000/api/mail/send \
    -H "Content-Type: application/json" \
    -d '{"to":["test@example.com"],"subject":"Test","body":"Test"}'
  echo "Request $i"
done
```

### Using Apache Bench

```bash
# Send 200 requests with 10 concurrent connections
ab -n 200 -c 10 http://localhost:5000/api/health

# Test mail endpoint (strict policy)
ab -n 50 -c 5 -p payload.json -T application/json http://localhost:5000/api/mail/send
```

## Troubleshooting

### Issue: Rate limit too restrictive

**Solution:** Increase `PermitLimit` or change to token bucket policy for burst support.

```csharp
[EnableRateLimiting("token")]  // Allows bursts
```

### Issue: Rate limit bypassed by changing IP

**Solution:** Use user-based or API key-based partition keys instead of IP-based.

```csharp
partitionKey: httpContext.User.FindFirst("sub")?.Value ?? "anonymous"
```

### Issue: Legitimate traffic blocked

**Solution:** Whitelist specific IPs or implement tiered rate limits.

```csharp
if (httpContext.Connection.RemoteIpAddress?.ToString() == "trusted-ip")
{
    return RateLimitPartition.GetNoLimiter("trusted");
}
```

### Issue: 429 responses not informative

**Solution:** Customize `OnRejected` handler with detailed messages.

```csharp
options.OnRejected = async (context, token) =>
{
    var response = new 
    {
        error = "Rate limit exceeded",
        limit = 100,
        window = "1 minute",
        retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) 
            ? retry.TotalSeconds 
            : 60
    };
    
    context.HttpContext.Response.StatusCode = 429;
    await context.HttpContext.Response.WriteAsJsonAsync(response, token);
};
```

## Security Considerations

1. **DDoS Protection**: Rate limiting helps prevent DDoS attacks by limiting requests per IP
2. **Brute Force Prevention**: Strict policies on auth endpoints prevent brute force attacks
3. **Resource Protection**: Prevents single clients from consuming all server resources
4. **Cost Management**: Reduces infrastructure costs from excessive API usage

## Performance Impact

- **Fixed Window**: ~1-2ms overhead per request
- **Token Bucket**: ~2-3ms overhead per request
- **Sliding Window**: ~3-5ms overhead per request
- **Memory**: ~100-500 bytes per partition key

## Future Enhancements

- Redis-based distributed rate limiting for multi-instance deployments
- User tier-based rate limits (free, premium, enterprise)
- Dynamic rate limit adjustment based on server load
- Rate limit analytics and reporting dashboard
- API key-based rate limiting with per-key limits

## Related Documentation

- [Redis Configuration](redis.md) - For distributed rate limiting
- [JWT Authentication](jwt.md) - For user-based rate limiting
- [Docker Setup](docker.md) - For environment variable configuration
