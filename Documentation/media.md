# Media Service with MinIO

A comprehensive media service powered by MinIO object storage and ImageSharp for image optimization.

## 🚀 Features

- ✅ File upload/download (single/multiple)
- ✅ S3-compatible storage with MinIO (Docker)
- ✅ Image optimization (ImageSharp)
- ✅ Automatic thumbnail generation
- ✅ On-demand image resizing
- ✅ File size and extension validation
- ✅ Folder-based organization
- ✅ JWT-based access control
- ✅ Direct public URLs for uploaded files

## 🏗️ Architecture

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│ Client  │──▶│   API   │──▶│  MinIO  │
└──────────┘     │         │     │ Docker  │
                │ImageSharp     └──────────┘
                │Processing│
                └──────────┘
```

## ⚙️ Configuration

### Docker Setup

MinIO is automatically configured in `docker-compose.dev.yml` and `docker-compose.prod.yml`:

```yaml
minio:
  image: minio/minio:latest
  ports:
    - "9000:9000"  # API
    - "9001:9001"  # Web Console
  environment:
    MINIO_ROOT_USER: minioadmin
    MINIO_ROOT_PASSWORD: minioadmin123
```

### Environment Variables

```env
Media__Endpoint=http://minio:9000
Media__AccessKey=minioadmin
Media__SecretKey=minioadmin123
Media__BucketName=uploads
Media__Region=us-east-1
Media__UseSSL=false
Media__PublicUrl=http://localhost:9000
Media__MaxFileSize=10485760
```

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `Endpoint` | MinIO server address | `http://localhost:9000` |
| `AccessKey` | MinIO access key | `minioadmin` |
| `SecretKey` | MinIO secret key | `minioadmin123` |
| `BucketName` | Default storage bucket | `uploads` |
| `PublicUrl` | Public file access URL | `http://localhost:9000` |
| `MaxFileSize` | Maximum file size (bytes) | `10485760` (10MB) |
| `ThumbnailWidth` | Thumbnail width | `200` |
| `ThumbnailHeight` | Thumbnail height | `200` |
| `MaxImageWidth` | Maximum image width | `1920` |
| `MaxImageHeight` | Maximum image height | `1080` |
| `JpegQuality` | JPEG compression quality | `85` |

## 📡 API Endpoints

### 1. Upload File

Single file upload:

```http
POST /api/media/upload?folder=images&generateThumbnail=true
Content-Type: multipart/form-data

file: [binary file]
```

**Query Parameters:**
- `folder` (optional): Target folder for upload
- `generateThumbnail` (optional): Generate thumbnail for images

**Response:**
```json
{
  "success": true,
  "fileName": "images/550e8400-e29b-41d4-a716-446655440000.jpg",
  "url": "http://localhost:9000/uploads/images/550e8400-e29b-41d4-a716-446655440000.jpg",
  "thumbnailUrl": "http://localhost:9000/uploads/images/thumbnails/550e8400-e29b-41d4-a716-446655440000_thumb.jpg",
  "fileSize": 245760,
  "message": "File uploaded successfully"
}
```

### 2. Upload Multiple Files

Upload multiple files at once.

```http
POST /api/media/upload/multiple?folder=documents
Content-Type: multipart/form-data

files: [file1, file2, file3]
```

**Response:**
```json
{
  "results": [
    {
      "success": true,
      "fileName": "documents/file1.pdf",
      "url": "http://localhost:9000/uploads/documents/file1.pdf",
      "fileSize": 128000,
      "message": "File uploaded successfully"
    }
  ],
  "successCount": 2,
  "failureCount": 1
}
```

### 3. Get File

Download or view a file.

```http
GET /api/media/{fileName}
```

**Example:**
```http
GET /api/media/images/550e8400-e29b-41d4-a716-446655440000.jpg
```

Returns the file as binary data with appropriate Content-Type.

### 4. Delete File

Delete a file from storage.

```http
DELETE /api/media/{fileName}
```

**Response:**
```json
{
  "success": true,
  "message": "File deleted successfully"
}
```

### 5. List Files

List all files in storage or a specific folder.

```http
GET /api/media/list?folder=images
```

**Response:**
```json
{
  "files": [
    {
      "fileName": "images/550e8400-e29b-41d4-a716-446655440000.jpg",
      "url": "http://localhost:9000/uploads/images/550e8400-e29b-41d4-a716-446655440000.jpg",
      "size": 0
    }
  ],
  "totalCount": 1,
  "folder": "images"
}
```

### 6. Resize Image

Resize an existing image.

```http
POST /api/media/resize
Content-Type: application/json

{
  "fileName": "images/original.jpg",
  "width": 800,
  "height": 600,
  "outputFolder": "resized"
}
```

**Response:**
```json
{
  "success": true,
  "resizedFileName": "resized/661f9511-f3ac-52e5-b827-557766551111_resized.jpg",
  "url": "http://localhost:9000/uploads/resized/661f9511-f3ac-52e5-b827-557766551111_resized.jpg",
  "message": "Image resized successfully"
}
```

## 🎯 Usage Examples

### C# / .NET

```csharp
using var client = new HttpClient();
using var content = new MultipartFormDataContent();
using var fileStream = File.OpenRead("photo.jpg");
using var streamContent = new StreamContent(fileStream);

content.Add(streamContent, "file", "photo.jpg");

var response = await client.PostAsync(
    "http://localhost:5143/api/media/upload?folder=photos&generateThumbnail=true",
    content
);

var result = await response.Content.ReadFromJsonAsync<UploadResponseDto>();
Console.WriteLine($"URL: {result.Url}");
Console.WriteLine($"Thumbnail: {result.ThumbnailUrl}");
```

### JavaScript / Fetch

```javascript
const formData = new FormData();
const fileInput = document.querySelector('#fileInput');
formData.append('file', fileInput.files[0]);

const response = await fetch('http://localhost:5143/api/media/upload?folder=images&generateThumbnail=true', {
  method: 'POST',
  body: formData
});

const result = await response.json();
console.log('File URL:', result.url);
console.log('Thumbnail URL:', result.thumbnailUrl);
```

### cURL

```bash
# Upload file
curl -X POST "http://localhost:5143/api/media/upload?folder=images&generateThumbnail=true" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@photo.jpg"

# Download file
curl -O "http://localhost:9000/uploads/images/550e8400-e29b-41d4-a716-446655440000.jpg"

# Delete file
curl -X DELETE "http://localhost:5143/api/media/images/550e8400-e29b-41d4-a716-446655440000.jpg"

# List files
curl "http://localhost:5143/api/media/list?folder=images"
```

## 🖼️ Image Processing

### Automatic Optimization

When uploading images, the service automatically:
- Resizes oversized images (max 1920x1080)
- Compresses JPEG with 85% quality
- Maintains aspect ratio
- Optimizes file size

### Thumbnail Generation

Enable `generateThumbnail=true` to automatically create 200x200 thumbnails:
- Stored in `{folder}/thumbnails/` subdirectory
- Cropped to fit (not stretched)
- JPEG format with 75% quality

### On-Demand Resizing

Use the resize endpoint to create custom sizes:

```json
POST /api/media/resize
{
  "fileName": "original.jpg",
  "width": 400,
  "height": 300
}
```

## 🔧 MinIO Management

### Web Console

Access MinIO's web console at: `http://localhost:9001`

**Credentials:**
- Username: `minioadmin`
- Password: `minioadmin123`

### Features:
- Browse buckets and files
- Upload files manually
- Set access policies
- Monitor storage usage
- Manage users and permissions

## 🚀 Getting Started

1. **Start services:**
```bash
make dev
# or
docker-compose -f Docker/docker-compose.dev.yml up
```

2. **Access services:**
- API: http://localhost:5143
- MinIO Console: http://localhost:9001
- Swagger: http://localhost:5143/swagger

3. **Upload your first file:**
```bash
curl -X POST "http://localhost:5143/api/media/upload?folder=test" \
  -F "file=@myfile.jpg"
```

## 🔐 Security Best Practices

### Production Recommendations:

1. **Change default credentials:**
```yaml
MINIO_ROOT_USER: your-secure-username
MINIO_ROOT_PASSWORD: your-secure-password-min-8-chars
```

2. **Enable SSL:**
```env
Media__UseSSL=true
Media__Endpoint=https://your-domain.com
```

3. **Add authentication to endpoints:**
```csharp
[Authorize] // Add JWT authentication
[HttpPost("upload")]
public async Task<ActionResult> UploadFile(...)
```

4. **Restrict file types:**
```csharp
AllowedExtensions = new[] { ".jpg", ".png", ".pdf" }
```

5. **Set up CORS properly:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder
            .WithOrigins("https://yourdomain.com")
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

6. **Use signed URLs for private files**

7. **Implement rate limiting** (already included in project)

## 📊 Allowed File Types

**Images:**
- `.jpg`, `.jpeg`, `.png`, `.gif`, `.svg`, `.webp`, `.bmp`

**Documents:**
- `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`

**Other:**
- `.txt`, `.csv`, `.zip`, `.rar`

## 🐛 Troubleshooting

### MinIO connection failed

```bash
# Check if MinIO is running
docker ps | grep minio

# Check MinIO logs
docker logs minio

# Restart MinIO
docker restart minio
```

### Bucket creation failed

The bucket is automatically created on startup. If it fails:
1. Check MinIO logs
2. Verify credentials in environment variables
3. Ensure MinIO is healthy: `docker ps`

### Files not accessible

1. Verify bucket policy is set to public read
2. Check `PublicUrl` configuration
3. Ensure port 9000 is accessible

### Image processing errors

1. Verify ImageSharp package is installed
2. Check file format is supported
3. Ensure sufficient memory for large images

## 🔄 Migration to AWS S3

The service uses MinIO which is S3-compatible. To migrate to AWS S3:

1. Change configuration:
```env
Media__Endpoint=s3.amazonaws.com
Media__AccessKey=YOUR_AWS_ACCESS_KEY
Media__SecretKey=YOUR_AWS_SECRET_KEY
Media__BucketName=your-bucket-name
Media__Region=us-east-1
Media__UseSSL=true
```

2. No code changes needed! The MinIO client works with AWS S3.

## 📈 Future Enhancements

- [ ] Video thumbnail generation
- [ ] WebP conversion for better compression
- [ ] Media integration (CloudFlare, CloudFront)
- [ ] Advanced image filters (blur, brightness, etc.)
- [ ] Virus scanning for uploads
- [ ] Quota management per user
- [ ] Automatic cleanup of old files
- [ ] Image watermarking
- [ ] Facial recognition and tagging
- [ ] Analytics and usage statistics

## 🤝 Integration with Other Services

### With JWT Authentication

```csharp
[Authorize] // Requires authentication
[HttpPost("upload")]
public async Task<ActionResult> UploadFile(...)
```

### With RabbitMQ (Async Processing)

Process large images asynchronously:
```csharp
await _rabbitMqService.PublishAsync("image-processing", new
{
    FileName = fileName,
    Operations = new[] { "resize", "watermark", "compress" }
});
```

### With Redis (Caching)

Cache frequently accessed files:
```csharp
var cachedFile = await _redisService.GetAsync($"media:{fileName}");
if (cachedFile != null) return File(cachedFile, contentType);
```

## 📝 License

This Media service is part of the .NET Boilerplate project.
