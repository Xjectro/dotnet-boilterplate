using Cassandra.Mapping.Attributes;

namespace Source.Models;

[Table("media_files")]
public class MediaModel
{
    [PartitionKey]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("file_name")]
    public string FileName { get; set; } = string.Empty;

    [Column("original_name")]
    public string OriginalName { get; set; } = string.Empty;

    [Column("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("folder")]
    public string? Folder { get; set; }

    [Column("url")]
    public string Url { get; set; } = string.Empty;

    [Column("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [Column("file_extension")]
    public string FileExtension { get; set; } = string.Empty;

    [Column("is_image")]
    public bool IsImage { get; set; }

    [Column("width")]
    public int? Width { get; set; }

    [Column("height")]
    public int? Height { get; set; }

    [Column("uploaded_at")]
    public DateTimeOffset UploadedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
