namespace Source.Configurations;

public class CdnSettings
{
    public string Endpoint { get; set; } = "http://localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin123";
    public string BucketName { get; set; } = "uploads";
    public string Region { get; set; } = "us-east-1";
    public bool UseSSL { get; set; } = false;
    public string PublicUrl { get; set; } = "http://localhost:9000";
    public long MaxFileSize { get; set; } = 10485760; // 10MB
    public string[] AllowedExtensions { get; set; } = new[] 
    { 
        ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp", ".bmp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".zip", ".rar"
    };
    public int ThumbnailWidth { get; set; } = 200;
    public int ThumbnailHeight { get; set; } = 200;
    public int MaxImageWidth { get; set; } = 1920;
    public int MaxImageHeight { get; set; } = 1080;
    public int JpegQuality { get; set; } = 85;
}
