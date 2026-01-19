using Cassandra.Mapping.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Source.Models;

[Table("clients")]
public class ClientModel
{
    [PartitionKey]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
    public string Name { get; set; } = "";
}
