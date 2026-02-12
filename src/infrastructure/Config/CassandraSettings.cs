namespace Api.Infrastructure.Config;

public class CassandraSettings
{
    public string ContactPoints { get; set; } = string.Empty;
    public string Keyspace { get; set; } = string.Empty;
    public int Port { get; set; } = 9042;
    public string Datacenter { get; set; } = "datacenter1";
    public string Consistency { get; set; } = "LocalQuorum";
    public string ReplicationClass { get; set; } = "SimpleStrategy";
    public int ReplicationFactor { get; set; } = 1;
}
