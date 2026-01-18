# Serilog + Seq Logging

## 📋 Overview

Structured logging implementation using **Serilog** with **Seq** as the log viewer.

## 🎯 Features

- **Structured Logging**: JSON-formatted logs with context
- **Multiple Sinks**: Console, File, Seq
- **Request Logging**: Automatic HTTP request/response logging
- **Log Enrichment**: Machine name, Thread ID, Request context
- **File Rolling**: Daily log files with 30-day retention
- **Real-time Viewing**: Seq UI for live log monitoring

## 🔧 Configuration

### Log Levels
- **Information**: General application flow
- **Warning**: Abnormal behavior that doesn't cause failure
- **Error**: Errors and exceptions
- **Fatal**: Critical failures causing shutdown

### Sinks

**1. Console**
```
[HH:mm:ss INF] Message {Property}
```

**2. File** (`logs/log-YYYYMMDD.txt`)
```
[yyyy-MM-dd HH:mm:ss.fff zzz] [INF] Message {Property}
```

**3. Seq** (http://localhost:5341)
- Structured JSON format
- Searchable properties
- Real-time updates

## 📊 Seq Dashboard

Access Seq at: **http://localhost:5341**

### Features:
- **Query Language**: SQL-like syntax for filtering
- **Signals**: Create alerts for specific patterns
- **Dashboards**: Visualize metrics and trends
- **Export**: Download logs in various formats

### Example Queries:
```sql
-- Find all errors
@Level = 'Error'

-- Find slow requests (> 1000ms)
@Level = 'Information' and Elapsed > 1000

-- Find requests from specific IP
RemoteIP = '172.20.0.1'

-- Find file upload errors
RequestPath like '/media/upload%' and @Level = 'Error'
```

## 🚀 Usage

### In Code
```csharp
// ILogger is automatically injected
private readonly ILogger<MyClass> _logger;

// Log with structured data
_logger.LogInformation("User {UserId} uploaded file {FileName}", userId, fileName);

// Log errors
_logger.LogError(ex, "Failed to process request");

// Log with properties
_logger.LogWarning("Rate limit exceeded for IP {IP}", ipAddress);
```

### HTTP Request Logs
Automatically logged for all requests:
```
HTTP GET /api/health responded 200 in 12.3456 ms
```

Includes:
- Request method and path
- Status code
- Response time
- Remote IP
- Request host

## 📂 Log Files

Location: `logs/` directory

Format: `log-YYYYMMDD.txt`

Retention: 30 days

Example:
```
logs/
├── log-20260119.txt
├── log-20260118.txt
└── log-20260117.txt
```

## 🐳 Docker Setup

Seq runs as a container:
```yaml
seq:
  image: datalust/seq:latest
  ports:
    - "5341:80"
  environment:
    ACCEPT_EULA: "Y"
```

## 🔍 Monitoring

### Key Metrics to Watch:
- Error rate (`@Level = 'Error'`)
- Slow requests (`Elapsed > 1000`)
- Failed uploads (`RequestPath like '/media/upload%' and @Level = 'Error'`)
- Authentication failures
- Rate limit hits

### Alerts (in Seq):
1. Create a Signal
2. Set query condition
3. Configure notification (email, webhook, etc.)

## 📈 Performance

- **Async Logging**: Non-blocking
- **Batching**: Logs sent in batches to Seq
- **File Buffering**: Efficient disk writes
- **Minimal Overhead**: < 1ms per log entry

## 🛠️ Troubleshooting

**Seq not receiving logs:**
- Check `SEQ_URL` environment variable
- Verify Seq container is running
- Check network connectivity

**Too many logs:**
- Increase minimum level in Program.cs
- Use filters to exclude noisy sources

**Disk space issues:**
- Adjust retention policy
- Reduce log file count

## 🔐 Production Recommendations

1. **Secure Seq**: Add authentication
2. **External Storage**: Mount Seq data volume
3. **Log Rotation**: Configure retention based on disk space
4. **Monitoring**: Set up alerts for critical errors
5. **API Keys**: Use Seq API keys for different apps

## 📚 Resources

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
