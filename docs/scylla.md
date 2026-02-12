# ScyllaDB Usage

## Overview
This project uses [ScyllaDB](https://www.scylladb.com/), a Cassandra-compatible NoSQL database, for distributed data storage. The .NET stack connects to ScyllaDB via the Cassandra .NET driver, so configuration sections and service names retain the `Cassandra` prefix for compatibility.

## Configuration

### appsettings.json
```json
{
  "Cassandra": {
    "ContactPoints": "scylla",
    "Port": 9042,
    "Datacenter": "istanbul",
    "Keyspace": "default_keyspace",
    "Consistency": "LocalQuorum",
    "ReplicationClass": "SimpleStrategy",
    "ReplicationFactor": 1
  }
}
```

### Environment Variables (Docker)
```env
Cassandra__ContactPoints=scylla
Cassandra__Port=9042
Cassandra__Datacenter=istanbul
Cassandra__Keyspace=default_keyspace
Cassandra__Consistency=LocalQuorum
Cassandra__ReplicationClass=SimpleStrategy
Cassandra__ReplicationFactor=1
```

> **Note:** The application still reads configuration from the `Cassandra` section because the ScyllaDB integration uses Cassandra driver abstractions.

## Service Implementation

### CassandraService.cs
Location: `src/infrastructure/Persistence/Cassandra/CassandraService.cs`

**Key Features:**
- Automatic cluster and session management for ScyllaDB
- Keyspace initialization on startup
- Connection pooling and retry policies
- Async query execution helpers

### Interface Methods
```csharp
public interface ICassandraService
{
    ISession GetSession();
    Task InitializeKeyspaceAsync();
    Task SeedDataAsync();
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

### 2. Execute Query
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

### 3. Create Table
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

The keyspace is automatically initialized on application startup and seeded when required:

**Program.cs:**
```csharp
await app.InitializeCassandraAsync();
await app.SeedCassandraDataAsync();
```

These helpers live in `src/presentation/api/DependencyInjection/ServiceExtensions.cs` and wrap the ScyllaDB initialization logic.

## Best Practices

1. **Use Prepared Statements** for repeated queries.
2. **Design Partition Keys Carefully** to distribute data evenly across Scylla nodes.
3. **Leverage Async APIs** to maximize throughput.
4. **Tune Consistency Levels** (`LocalQuorum`, `Quorum`, etc.) per workload.
5. **Monitor Cluster Health** via Scylla Monitoring Stack or nodetool equivalents.

## Connection Management

- The Cassandra driver handles connection pooling and node discovery.
- Sessions are managed as singletons through dependency injection.
- Automatic reconnection strategies retry failed requests to other nodes.
- Health checks verify connectivity on container startup.

## Error Handling

```csharp
try
{
    var session = _cassandraService.GetSession();
    await session.ExecuteAsync(statement);
}
catch (NoHostAvailableException ex)
{
    _logger.LogError(ex, "Scylla cluster unavailable");
}
catch (QueryExecutionException ex)
{
    _logger.LogError(ex, "Scylla query execution failed");
}
```

## Docker Integration

The ScyllaDB container is defined in `deploy/docker/dev/docker-compose.yml`:
- Uses the `scylladb/scylla:latest` image
- Persists data to the `cassandra_data` volume
- Exposes CQL port `9042`
- Includes a basic health check using `cqlsh`

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

Refer to the official ScyllaDB documentation for performance tuning, scaling, and production deployment guidance.
