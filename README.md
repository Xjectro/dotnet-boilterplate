
## Dotnet Boilerplate - .NET 10 Web API (ScyllaDB, Kafka, Redis, Kestrel)
A modern, high-performance boilerplate for authentication, session management, and distributed data using ScyllaDB, Kafka, Redis, and Kestrel. Saniyede 1 milyon istek hedefiyle optimize edilmiştir.


### Features
- JWT authentication
- User registration & login
- BCrypt password hashing
- ScyllaDB (Cassandra uyumlu, yüksek performanslı NoSQL)
- Kafka (yüksek hacimli mesajlaşma)
- Redis (cache ve hızlı veri erişimi)
- Kestrel (yüksek performanslı .NET web sunucusu)
- Clean architecture
- Swagger API docs

### Quick Start
1. **Clone & Enter Project**
   ```bash
   git clone <repo-url>
   cd healthy-practice
   ```

2. **Start All Services (Docker Compose)**
   ```bash
   docker-compose up --build -d
   ```


3. **Config & Environment**
   `Configurations/appsettings.yml` dosyasında örnek ayarlar:
   ```yaml
   ScyllaDb:
     Host: "scylla"
   Kafka:
     Host: "kafka:9092"
   Redis:
     Host: "redis:6379"
   ```

4. **Build & Run (Manual)**
   ```bash
   dotnet build
   dotnet run
   ```

### ScyllaDB ile CRUD ve Kafka ile Sıralama

#### MemberRepository (ScyllaDB)
```csharp
var session = await scyllaService.GetSessionAsync();
var repo = new MemberRepository(session);
await repo.AddAsync(new Member("user", "mail", "pass"));
```

#### Kafka ile istekleri sıraya sokmak
```csharp
await kafkaQueueService.PublishMemberRequestAsync(member);
var consumed = await kafkaQueueService.ConsumeMemberRequestAsync();
if (consumed != null)
    await repo.AddAsync(consumed);
```

### Örnek Kullanım (C#)

#### ScyllaDB
```csharp
var session = await scyllaService.GetSessionAsync();
// CQL sorguları ile veri işle
```

#### Kafka
```csharp
await kafkaService.ProduceAsync("topic", "mesaj");
var msg = await kafkaService.ConsumeAsync("topic", "group1");
```

#### Redis
```csharp
await redisService.SetAsync("key", "value");
var value = await redisService.GetAsync("key");
```

### Performans
- Kestrel limits ayarları ile 1 milyon eşzamanlı bağlantı desteklenir.
- Docker Compose ile tüm servisler izole ve yüksek performanslı çalışır.


### API Endpoints
- `POST /auth/register` — Register user
- `POST /auth/login` — Login, returns JWT
- `GET /session` — Get current user (JWT required)

### Project Structure
```
Source/
├── Controllers/
├── Middleware/
├── DTOs/
├── Models/
├── Repositories/
├── Services/
├── Docs/
├── Program.cs
├── Api.csproj
```


### Docs & SQL
See `Docs/auth.md` and `Docs/session.md` for details and example SQL.

### Distributed Services
- ScyllaDB: Yüksek performanslı NoSQL veri saklama
- Kafka: Dağıtık mesaj kuyruğu
- Redis: Hızlı cache ve key-value store

dotnet build
dotnet run

### License
Custom educational license. See LICENCE file.

The API will be available at:
- HTTP: `http://localhost:5143` (or your configured port)


## 📚 API Endpoints

See `Docs/auth.md` and `Docs/session.md` for details, example requests, and SQL setup.


## 🔧 Development Commands

Common .NET CLI commands:
```bash
# Build project
dotnet build

# Run project
dotnet run
```


## 🗂️ Project Structure

```
├── Source/
│   ├── Controllers/        # API controllers
│   ├── Middleware/         # Custom middleware
│   ├── Attributes/         # Custom attributes
│   ├── DTOs/               # Data Transfer Objects
│   ├── Models/             # Entity models
│   ├── Repositories/       # Data access (ADO.NET)
│   └── Services/           # Business logic services
├── Configurations/         # YAML configuration files
├── Docs/                   # Documentation (auth, session, SQL)
├── Program.cs              # Main entry point
├── Api.csproj              # Project file
└── README.md               # This file
```

## 🔒 Security Features

- **JWT Token Authentication**: Stateless authentication mechanism
- **BCrypt Password Hashing**: Industry-standard password encryption (cost factor: 12)
- **Authorization Middleware**: Custom middleware for token validation
- **Secure Headers**: Proper security headers configuration

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to `http://localhost:5143/swagger` when the application is running
2. Use the interactive documentation to test endpoints
3. For protected endpoints, click "Authorize" and enter: `Bearer <your-token>`

### Using HTTP Files
The project includes `src/Api/Api.http` file for testing with REST clients.


## 📝 Database Schema
See `Docs/auth.md` for the latest SQL schema for the members table.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under a custom educational license. See the [LICENCE](LICENCE) file for details.

**Note**: This software is provided for learning and educational purposes only. Commercial use, distribution, or sale is strictly prohibited.

## 🚨 Important Security Notes

- Change the default JWT secret key in production
- Use strong passwords for database connections
- Never commit sensitive configuration to version control
- Consider using environment variables for sensitive settings
- Implement rate limiting in production
- Add HTTPS configuration for production deployment

## 📞 Support

For questions and support, please create an issue in the repository.