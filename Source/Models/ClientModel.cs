using Cassandra.Mapping.Attributes;

namespace Source.Models;

[Table("clients")]
public class ClientModel
{
    [PartitionKey]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";
}
