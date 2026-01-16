# Cassandra Database Usage

## Overview
This project uses Apache Cassandra as the primary NoSQL database for distributed data storage.

## Configuration

### appsettings.json
```json
{
  "Cassandra": {
    "ContactPoints": "cassandra",
    "Port": 9042,
    "Datacenter": "istanbul",
    "Keyspace": "default_keyspace",
    "Consistency": "LocalQuorum",
    "ReplicationClass": "SimpleStrategy",
    "ReplicationFactor": "1"
  }
}
```

### Environment Variables (Docker)
```env
Cassandra__ContactPoints=cassandra
Cassandra__Port=9042
Cassandra__Datacenter=istanbul
Cassandra__Keyspace=default_keyspace
Cassandra__Consistency=LocalQuorum
Cassandra__ReplicationClass=SimpleStrategy
Cassandra__ReplicationFactor=1
```

## Service Implementation

### CassandraService.cs
Located at: `Source/Services/CassandraService/CassandraService.cs`

**Key Features:**
- Automatic session management
- Keyspace initialization
- Connection pooling
- Query execution

### Interface Methods
```csharp
public interface ICassandraService
{
    ISession GetSession();
    Task InitializeKeyspaceAsync();
}
```

## Usage Examples

### 1. Inject the Service
```csharp
public class ClientRepository
{
    private readonly ICassandraService _cassandraService;
    
    public ClientRepository(ICassandraService cassandraService)
    {
        _cassandraService = cassandraService;
    }
}
```

### 2. Execute Queries
```csharp
public async Task<Client> GetClientAsync(Guid id)
{
    var session = _cassandraService.GetSession();
    var statement = new SimpleStatement(
        "SELECT * FROM clients WHERE id = ?", id
    );
    
    var result = await session.ExecuteAsync(statement);
    return result.FirstOrDefault();
}
```

### 3. Create Tables
```csharp
public async Task CreateTableAsync()
{
    var session = _cassandraService.GetSession();
    await session.ExecuteAsync(new SimpleStatement(@"
        CREATE TABLE IF NOT EXISTS clients (
            id UUID PRIMARY KEY,
            name TEXT,
            email TEXT,
            created_at TIMESTAMP
        )
    "));
}
```

## Initialization

The keyspace is automatically initialized on application startup:

**Program.cs:**
```csharp
await app.InitializeCassandraAsync();
```

This ensures the keyspace exists before any queries are executed.

## Best Practices

1. **Use Prepared Statements**: For repeated queries
2. **Batch Operations**: Group related inserts/updates
3. **Partition Keys**: Design tables with proper partition keys
4. **Consistency Levels**: Use appropriate consistency for your use case
5. **Async Operations**: Always use async methods

## Connection Management

- Sessions are singleton (one per application)
- Connections are automatically pooled
- Reconnection is handled automatically
- Health checks verify connectivity

## Error Handling

```csharp
try
{
    var session = _cassandraService.GetSession();
    await session.ExecuteAsync(statement);
}
catch (NoHostAvailableException ex)
{
    _logger.LogError(ex, "Cassandra cluster unavailable");
}
catch (QueryExecutionException ex)
{
    _logger.LogError(ex, "Query execution failed");
}
```

## Docker Integration

Cassandra container is defined in `docker-compose.dev.yml`:
- Automatic cluster setup
- Data persistence with volumes
- Health checks included

## Useful CQL Commands

```cql
-- List keyspaces
DESCRIBE KEYSPACES;

-- Use keyspace
USE default_keyspace;

-- Show tables
DESCRIBE TABLES;

-- Show table structure
DESCRIBE TABLE clients;
```
