# .NET Boilerplate

A production-ready .NET 10 boilerplate with microservices architecture, featuring ScyllaDB (Cassandra-compatible), Redis, RabbitMQ, and comprehensive service implementations.

## 🚀 Features

- ✅ **RESTful API** with Swagger/ReDoc documentation
- ✅ **ScyllaDB (Cassandra-compatible)** for distributed NoSQL storage
- ✅ **Redis** for high-performance caching
- ✅ **RabbitMQ** for asynchronous message processing
- ✅ **JWT Authentication** for secure API access
- ✅ **Rate Limiting** with multiple strategies (Fixed Window, Token Bucket, Sliding Window)
- ✅ **Mail Service** with queue-based async sending
- ✅ **Media Service** with MinIO object storage and ImageSharp optimization
- ✅ **Worker Service** for background job processing
- ✅ **Logging** with Serilog and Seq dashboard
- ✅ **Docker** containerization with Docker Compose
- ✅ **Health Checks** for all services
- ✅ **BCrypt** password hashing
- ✅ **Repository Pattern** with dependency injection

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

## 🏗️ Architecture

```
┌─────────────┐     ┌──────────┐     ┌───────────┐
│   Client    │────▶│   API    │────▶│ ScyllaDB  │
└─────────────┘     └────┬─────┘     └───────────┘
                         │
                    ┌────┴──────┐
                    │           │
              ┌─────▼────┐ ┌────▼─────┐
              │  Redis   │ │ RabbitMQ │
              │  Cache   │ │  Queue   │
              └──────────┘ └────┬─────┘
                    │           │
              ┌─────▼────┐ ┌────▼──────┐
              │  MinIO   │ │  Worker   │
              │  Media   │ │  Service  │
              └──────────┘ └───────────┘
```

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
git clone https://github.com/Xjectro/dotnet-boilterplate.git
cd dotnet-boilterplate

make dev

# API: http://localhost:5143
# Swagger: http://localhost:5143/swagger
# MinIO Console: http://localhost:9001 (minioadmin/minioadmin123)
# RabbitMQ Management: http://localhost:15672 (admin/admin123)
```

### Local Development

```bash
# Restore dependencies
dotnet restore

# Run the API locally
dotnet run --project src/presentation/api/Api.csproj
```


## 📚 Documentation

Comprehensive documentation is available in the [docs](docs/) folder:

- **[System Overview](docs/README.md)** - Architecture and project structure
- **[ScyllaDB (Cassandra-compatible)](docs/scylla.md)** - Database setup and usage
- **[RabbitMQ Queue](docs/rabbitmq.md)** - Message queue implementation
- **[Mail Service](docs/mail-service.md)** - Email service with async processing
- **[Media Service](docs/media.md)** - File upload, storage and delivery with MinIO
- **[Redis Cache](docs/redis.md)** - Caching strategies and usage
- **[JWT Authentication](docs/jwt.md)** - Security and authentication
- **[Rate Limiting](docs/rate-limiting.md)** - API throttling and DDoS protection
- **[Logging (Serilog + Seq)](docs/logging.md)** - Structured logging and monitoring
- **Validation (FluentValidation)** - Model and request validation
- **[Docker Setup](docs/docker.md)** - Container orchestration

## 🛠️ Configuration

### Environment Variables

Key configuration can be set via environment variables:

```env
# JWT
JwtSettings__Secret=your-secret-key
JwtSettings__ExpiryMinutes=60

# Redis
Redis__Host=redis:6379

# Scylla (Cassandra-compatible settings)
Cassandra__ContactPoints=scylla
Cassandra__Port=9042
Cassandra__Keyspace=default_keyspace

# RabbitMQ
RabbitMq__Host=rabbitmq
RabbitMq__Port=5672
RabbitMq__Username=admin
RabbitMq__Password=admin123

# Media (MinIO)
Media__Endpoint=http://minio:9000
Media__AccessKey=minioadmin
Media__SecretKey=minioadmin123
Media__BucketName=uploads
Media__PublicUrl=http://localhost:9000
Media__MaxFileSize=10485760

```

## 📡 API Endpoints

### Mail Service
```http
POST /api/mail/send
Content-Type: application/json

{
      "to": ["user@example.com"],
      "subject": "Welcome!",
      "body": "<h1>Hello World</h1>",
      "isHtml": true
}
```

### Media Service
```http
# Upload file
POST /api/media/upload?folder=images&generateThumbnail=true
Content-Type: multipart/form-data

# Get file
GET /api/media/{fileName}

# Delete file
DELETE /api/media/{fileName}

# List files
GET /api/media/list?folder=images
```

### Client Management
```http
GET /api/client
POST /api/client
PUT /api/client/{id}
DELETE /api/client/{id}
```

Visit `/swagger` for complete API documentation.

## 🏗️ Project Structure

```
├── src/
│   ├── core/               # Domain entities and shared contracts
│   ├── application/        # Use cases, validation, and interfaces
│   ├── infrastructure/     # External adapters (DB, messaging, storage)
│   └── presentation/
│       └── api/            # ASP.NET Core API host and middleware
├── deploy/
│   └── docker/             # Dockerfiles and compose configurations
├── docs/                   # Additional documentation (guides, ADRs)
├── tests/                  # Unit and integration test projects
├── ops/                    # CI/CD pipelines and operational scripts
├── api.sln                 # Solution file
└── README.md
```

## 🔧 Development

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Format Code

```bash
dotnet format
```

## 🐳 Docker Commands

```bash
# Start development environment (with build)
make dev

# Start production environment (detached with build)
make prod

# View logs
docker compose -f deploy/docker/dev/docker-compose.yml logs -f

# Stop services
docker compose -f deploy/docker/dev/docker-compose.yml down

# Stop and remove volumes
docker compose -f deploy/docker/dev/docker-compose.yml down -v
```

## 📊 Services

### ScyllaDB
- **Port**: 9042
- **Keyspace**: default_keyspace
- **Replication**: SimpleStrategy (Dev)

### Redis
- **Port**: 6379
- **Persistence**: AOF enabled

### RabbitMQ
- **AMQP Port**: 5672
- **Management**: http://localhost:15672
- **Credentials**: admin/admin123

### API
- **Port**: 5143
- **Swagger**: http://localhost:5143/swagger
- **ReDoc**: http://localhost:5143/api-docs

## 🔐 Security

- JWT token-based authentication
- BCrypt password hashing
- Environment variable configuration
- Docker secrets support
- HTTPS ready

## 🚦 Health Checks

All services include health checks:

```bash
# API Health
curl http://localhost:5143/health

# Docker health status
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENCE](LICENCE) file for details.

## 📧 Contact

For questions or support, please open an issue on GitHub.

## 🙏 Acknowledgments

- .NET Team for the amazing framework
- Docker for containerization
- ScyllaDB for distributed database
- Redis for caching
- RabbitMQ for message queuing

---

**Built with ❤️ using .NET 10**
