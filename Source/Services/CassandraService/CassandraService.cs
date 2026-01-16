using Microsoft.Extensions.Options;
using Source.Configurations;
using Cassandra.Data.Linq;
using Source.Models;

namespace Source.Services.CassandraService;

public class CassandraService : ICassandraService
{
    private readonly Cassandra.ICluster _cluster;
    private readonly Cassandra.ISession _session;
    private readonly CassandraSettings _settings;

    public CassandraService(IOptions<CassandraSettings> options)
    {
        _settings = options.Value;

        _cluster = Cassandra.Cluster.Builder()
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

        await _session.ExecuteAsync(new Cassandra.SimpleStatement(createKeyspaceCql));
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

    public Cassandra.ISession GetSession()
    {
        return _session;
    }

    public Table<T> GetTable<T>() where T : class
    {
        return new Table<T>(_session);
    }

    public void Dispose()
    {
        _session.Dispose();
        _cluster.Dispose();
    }
}