# JWT Authentication

## Overview
JWT (JSON Web Token) provides secure and stateless user authentication.

## Configuration

### appsettings.json
```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "ExpiryMinutes": 60
  }
}
```

### Environment Variables (Docker)
```env
JwtSettings__Secret=your-secret-key-at-least-32-characters-long
JwtSettings__ExpiryMinutes=60
```

## Service Implementation

### JwtService.cs
Location: `Source/Services/JwtService/JwtService.cs`

**Key Features:**
- Token generation
- Token validation
- Claims management
- Configurable expiration

### Interface Methods
```csharp
public interface IJwtService
{
    string GenerateToken(string userId, string email, Dictionary<string, string>? additionalClaims = null);
    ClaimsPrincipal? ValidateToken(string token);
    string? GetClaimValue(ClaimsPrincipal principal, string claimType);
}
```

## Usage Examples

### 1. Generate Token
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Authenticate user
    var user = await ValidateUserAsync(request.Email, request.Password);
    if (user == null)
        return Unauthorized();
    // Generate JWT token
    var token = _jwtService.GenerateToken(
        userId: user.Id.ToString(),
        email: user.Email,
        additionalClaims: new Dictionary<string, string>
        {
            { "role", user.Role },
            { "name", user.Name }
        }
    );
    return Ok(new { token, expiresIn = 3600 });
}
```

### 2. Validate Token
```csharp
public ClaimsPrincipal? ValidateRequest(string authHeader)
{
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        return null;
    var token = authHeader.Substring("Bearer ".Length);
    return _jwtService.ValidateToken(token);
}
```

### 3. Extract Claims
```csharp
[HttpGet("profile")]
[Authorize]
public IActionResult GetProfile()
{
    var userId = _jwtService.GetClaimValue(User, ClaimTypes.NameIdentifier);
    var email = _jwtService.GetClaimValue(User, ClaimTypes.Email);
    var role = _jwtService.GetClaimValue(User, "role");
    return Ok(new { userId, email, role });
}
```

## Token Structure

A JWT consists of three parts separated by dots:
```
header.payload.signature
```

### Example Token
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### Decoded Payload
```json
{
  "sub": "user-id-123",
  "email": "user@example.com",
  "role": "admin",
  "name": "John Doe",
  "exp": 1735689600,
  "iat": 1735686000
}
```

## Claims

### Standard Claims
- `sub` (Subject): User ID
- `email`: User email
- `exp` (Expiration): Token expiry timestamp
- `iat` (Issued At): Token creation timestamp
- `jti` (JWT ID): Unique token identifier

### Custom Claims
Add any additional data:
```csharp
var claims = new Dictionary<string, string>
{
    { "role", "admin" },
    { "department", "IT" },
    { "permissions", "read,write,delete" }
};

var token = _jwtService.GenerateToken(userId, email, claims);
```

## Middleware Integration

### 1. Configure Authentication
```csharp
// Program.cs or Startup.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWT_SETTINGS:Secret"])
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add to pipeline
app.UseAuthentication();
app.UseAuthorization();
```

### 2. Protect Endpoints
```csharp
[Authorize]
[HttpGet("protected")]
public IActionResult ProtectedEndpoint()
{
    return Ok("This is a protected resource");
}

[Authorize(Roles = "Admin")]
[HttpDelete("users/{id}")]
public IActionResult DeleteUser(string id)
{
    return Ok("User deleted");
}
```

## Client Usage

### 1. Login and Store Token
```javascript
// Login request
const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
});

const { token } = await response.json();
localStorage.setItem('token', token);
```

### 2. Send Token with Requests
```javascript
const token = localStorage.getItem('token');

const response = await fetch('/api/protected', {
    headers: {
        'Authorization': `Bearer ${token}`
    }
});
```

## Security Best Practices

1. **Strong Secret**: Use at least 32 characters
2. **HTTPS Only**: Never send tokens over HTTP
3. **Short Expiry**: 15-60 minutes recommended
4. **Refresh Tokens**: Implement refresh token mechanism
5. **Secure Storage**: Use httpOnly cookies or secure storage
6. **Validate Always**: Check token on every request
7. **Rotate Secrets**: Change secret key periodically

## Token Refresh Pattern

```csharp
[HttpPost("refresh")]
public IActionResult RefreshToken([FromBody] RefreshRequest request)
{
    var principal = _jwtService.ValidateToken(request.ExpiredToken);
    if (principal == null)
        return Unauthorized();
    
    // Verify refresh token (from database)
    var storedRefreshToken = await _db.GetRefreshTokenAsync(request.RefreshToken);
    if (storedRefreshToken == null || storedRefreshToken.IsExpired)
        return Unauthorized();
    
    // Generate new access token
    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = principal.FindFirst(ClaimTypes.Email)?.Value;
    
    var newToken = _jwtService.GenerateToken(userId, email);
    
    return Ok(new { token = newToken });
}
```

## Logout Implementation

Since JWT is stateless, logout is handled client-side:

```javascript
// Client-side logout
localStorage.removeItem('token');
// Redirect to login
```

For server-side logout (token blacklist):
```csharp
[HttpPost("logout")]
[Authorize]
public async Task<IActionResult> Logout()
{
    var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    
    // Add to blacklist (Redis)
    await _redisService.SetAsync(
        $"blacklist:{token}", 
        true, 
        TimeSpan.FromMinutes(60) // Same as token expiry
    );
    
    return Ok(new { message = "Logged out successfully" });
}

// Middleware to check blacklist
public class TokenBlacklistMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            var isBlacklisted = await _redisService.ExistsAsync($"blacklist:{token}");
            if (isBlacklisted)
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
        await _next(context);
    }
}
```

## Error Handling

```csharp
try
{
    var token = _jwtService.GenerateToken(userId, email);
}
catch (SecurityTokenException ex)
{
    _logger.LogError(ex, "Token generation failed");
    return StatusCode(500, "Authentication error");
}

try
{
    var principal = _jwtService.ValidateToken(token);
}
catch (SecurityTokenExpiredException)
{
    return Unauthorized("Token expired");
}
catch (SecurityTokenInvalidSignatureException)
{
    return Unauthorized("Invalid token signature");
}
```

## Testing

### Generate Test Token
```csharp
[HttpPost("test/generate-token")]
#if DEBUG
public IActionResult GenerateTestToken()
{
    var token = _jwtService.GenerateToken(
        "test-user-123",
        "test@example.com",
        new Dictionary<string, string> { { "role", "admin" } }
    );
    return Ok(new { token });
}
#else
    return NotFound();
#endif
```

### Decode Token
Use https://jwt.io to decode and inspect tokens during development.

## Common Issues

### Token Not Validating
- Check secret key matches
- Verify token hasn't expired
- Ensure proper Bearer format

### Claims Not Found
- Check claim names match
- Verify claims were added during generation
- Use correct ClaimTypes constants

### Token Expiring Too Fast
- Adjust ExpiryMinutes in configuration
- Implement refresh token mechanism
- Consider longer expiry for internal services
