using Cassandra.Data.Linq;

namespace Api.Application.Common.Interfaces;

public interface ICassandraService : IDisposable
{
    Task InitializeKeyspaceAsync();
    Task InitializeTablesAsync();
    Task SeedDataAsync();
    Cassandra.ISession GetSession();
    Table<T> GetTable<T>() where T : class;
}
