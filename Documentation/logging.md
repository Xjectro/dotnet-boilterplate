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
# Serilog + Seq Logging

## 📋 Overview

Structured logging implementation using Serilog and live log monitoring with Seq UI.

## 🎯 Features

- Structured logs (JSON format)
- Multiple sinks: Console, File, Seq
- Automatic HTTP request/response logging
- Log enrichment (machine name, thread ID, request context)
- Daily log file rotation (30-day retention)
- Live monitoring with Seq UI

## 🔧 Configuration

### Log Levels
- Information: General flow
- Warning: Abnormal but non-error situations
- Error: Errors and exceptions
- Fatal: Critical errors causing shutdown

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
- JSON format
- Property search
- Real-time updates

## 📊 Seq Dashboard

Seq access address: **http://localhost:5341**

### Features:
- Query language (SQL-like)
- Signal/alert creation
- Dashboard and metric visualization
- Export logs

### Query Examples:
```sql
-- Find all errors
@Level = 'Error'

-- Slow requests (> 1000ms)
@Level = 'Information' and Elapsed > 1000

-- Requests from specific IP
RemoteIP = '172.20.0.1'

-- File upload errors
RequestPath like '/media/upload%' and @Level = 'Error'
```

## 🚀 Usage

### In Code
```csharp
// ILogger is automatically injected
private readonly ILogger<MyClass> _logger;

// Structured log
_logger.LogInformation("User {UserId} uploaded file: {FileName}", userId, fileName);

// Error log
_logger.LogError(ex, "Request failed");

// Property log
_logger.LogWarning("Rate limit exceeded: {IP}", ipAddress);
```

### HTTP Request Logs
All requests are automatically logged:
```
HTTP GET /api/health responded 200 in 12.3456 ms
```

Content:
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

## 🛠️ Troubleshooting

**Seq not receiving logs:**
- Check `SEQ_URL` environment variable
- Ensure Seq container is running
- Check network connection

**Too many logs:**
- Increase minimum level in Program.cs
- Filter noisy sources

**Disk full:**
- Adjust retention policy
- Reduce file count

## 🔐 Production Recommendations

1. Secure Seq (authentication)
2. Use external storage (volume mount)
3. Log rotation and disk management
4. Monitoring/alert for critical errors
5. Seq API keys for different apps

## 📚 Resources

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
4. **Monitoring**: Set up alerts for critical errors
5. **API Keys**: Use Seq API keys for different apps

## 📚 Resources

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
