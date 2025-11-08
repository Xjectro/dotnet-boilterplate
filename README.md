## Dotnet Boilerplate - .NET 10 Web API
A modern boilerplate for authentication and session management using ADO.NET and PostgreSQL.

### Features
- JWT authentication
- User registration & login
- BCrypt password hashing
- PostgreSQL database
- ADO.NET data access
- Clean architecture
- Swagger API docs

### Quick Start
1. **Clone & Enter Project**
   ```bash
   git clone <repo-url>
   cd healthy-practice
   ```
2. **Configure Database**
   Edit `Configurations/appsettings.yml`:
   ```yaml
   ConnectionStrings:
     DefaultConnection: "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
   JwtSettings:
     Secret: "your-secret-key"
     Issuer: "YourApp"
     Audience: "YourAppUsers"
     ExpiresInMinutes: 60
   ```
3. **Create Table**
   Run this SQL in your PostgreSQL database:
   ```sql
   CREATE TABLE members (
     id SERIAL PRIMARY KEY,
     username VARCHAR(30) NOT NULL,
     email VARCHAR(255) NOT NULL,
     password VARCHAR(255) NOT NULL
   );
   ```
4. **Build & Run**
   ```bash
   dotnet build
   dotnet run
   ```

### API Endpoints
- `POST /api/auth/register` — Register user
- `POST /api/auth/login` — Login, returns JWT
- `GET /api/session` — Get current user (JWT required)

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

### License
Custom educational license. See LICENCE file.
dotnet build

# Run the application
dotnet run
```


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