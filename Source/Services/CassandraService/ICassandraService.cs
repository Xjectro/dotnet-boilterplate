using Cassandra.Data.Linq;

namespace Source.Services.CassandraService;

public interface ICassandraService : IDisposable
{
    Task InitializeKeyspaceAsync();
    Task InitializeTablesAsync();
    Cassandra.ISession GetSession();
    Table<T> GetTable<T>() where T : class;
}