using Cassandra.Data.Linq;

namespace Source.Services.CassandraService;

public interface ICassandraService : IDisposable
{
    Task InitializeKeyspaceAsync();
    Task InitializeTablesAsync();
    Task SeedDataAsync();
    Cassandra.ISession GetSession();
    Table<T> GetTable<T>() where T : class;
}