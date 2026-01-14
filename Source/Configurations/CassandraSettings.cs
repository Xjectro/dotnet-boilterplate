namespace Source.Configurations;

public class CassandraSettings
{
    public string CONTACT_POINTS { get; set; } = string.Empty;
    public string KEYSPACE { get; set; } = string.Empty;
    public int PORT { get; set; } = 9042;
    public string DATACENTER { get; set; } = "datacenter1";
    public string CONSISTENCY { get; set; } = "LocalQuorum";
    public string REPLICATION_CLASS { get; set; } = "SimpleStrategy";
    public int REPLICATION_FACTOR { get; set; } = 1;
}