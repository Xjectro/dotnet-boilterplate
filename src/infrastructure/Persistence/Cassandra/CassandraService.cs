using Api.Application.Common.Interfaces;
using Api.Core.Domain;
using Api.Infrastructure.Config;
using Cassandra.Data.Linq;
using Microsoft.Extensions.Options;
using CassandraDriver = global::Cassandra;

namespace Api.Infrastructure.Persistence.Cassandra;

public class CassandraService : ICassandraService
{
    private readonly CassandraDriver.ICluster _cluster;
    private readonly CassandraDriver.ISession _session;
    private readonly CassandraSettings _settings;

    public CassandraService(IOptions<CassandraSettings> options)
    {
        _settings = options.Value;

        _cluster = CassandraDriver.Cluster.Builder()
            .AddContactPoints(_settings.ContactPoints.Split(','))
            .WithPort(_settings.Port)
            .Build();

        _session = _cluster.Connect();
    }

    public async Task InitializeKeyspaceAsync()
    {
        string replicationStrategy = _settings.ReplicationClass == "NetworkTopologyStrategy"
            ? $"'class': 'NetworkTopologyStrategy', '{_settings.Datacenter}': {_settings.ReplicationFactor}"
            : $"'class': 'SimpleStrategy', 'replication_factor': {_settings.ReplicationFactor}";

        string createKeyspaceCql = $@"
            CREATE KEYSPACE IF NOT EXISTS {_settings.Keyspace}
            WITH REPLICATION = {{ {replicationStrategy} }};";

        await _session.ExecuteAsync(new CassandraDriver.SimpleStatement(createKeyspaceCql));
        _session.ChangeKeyspace(_settings.Keyspace);

        // Automatically create all tables
        await InitializeTablesAsync();
    }

    public async Task InitializeTablesAsync()
    {
        // Add all model tables here
        var tables = new List<Action>
        {
            () => GetTable<ClientModel>().CreateIfNotExists(),
            () => GetTable<MediaModel>().CreateIfNotExists(),
            // New models will be added here
            // () => GetTable<YourNewModel>().CreateIfNotExists(),
        };

        await Task.Run(() =>
        {
            foreach (var createTable in tables)
            {
                createTable();
            }
        });
    }

    public CassandraDriver.ISession GetSession()
    {
        return _session;
    }

    public Table<T> GetTable<T>() where T : class
    {
        return new Table<T>(_session);
    }

    public async Task SeedDataAsync()
    {
        var clientTable = GetTable<ClientModel>();

        // Check if data already exists
        var existingClients = await clientTable.Take(1).ExecuteAsync();
        if (existingClients.Any())
        {
            // Data already seeded, skip
            return;
        }

        // Seed sample clients
        var sampleClients = new[]
        {
            new ClientModel { Id = Guid.NewGuid(), Name = "Acme Corporation" },
            new ClientModel { Id = Guid.NewGuid(), Name = "TechStart Inc" },
            new ClientModel { Id = Guid.NewGuid(), Name = "Global Solutions Ltd" },
            new ClientModel { Id = Guid.NewGuid(), Name = "Innovation Labs" },
            new ClientModel { Id = Guid.NewGuid(), Name = "Digital Ventures" }
        };

        foreach (var client in sampleClients)
        {
            await clientTable.Insert(client).ExecuteAsync();
        }

        Console.WriteLine($"Seeded {sampleClients.Length} sample clients to Cassandra");
    }

    public void Dispose()
    {
        _session.Dispose();
        _cluster.Dispose();
    }
}
