# Project Root - .NET Web API

A clean architecture .NET 9.0 Web API project with JWT authentication, built with Entity Framework Core and PostgreSQL.

## 🏗️ Architecture

This project follows Clean Architecture principles with the following layers:

- **Domain**: Core business entities and domain logic
- **Application**: Business logic, DTOs, and service interfaces
- **Infrastructure**: Data access, repositories, and external services
- **API**: Controllers, middleware, and API configuration

## 🚀 Features

- **JWT Authentication**: Secure token-based authentication
- **User Registration & Login**: Complete authentication flow
- **BCrypt Password Hashing**: Secure password storage
- **PostgreSQL Database**: Robust data persistence
- **Entity Framework Core**: Code-first database approach
- **Swagger Documentation**: Interactive API documentation
- **Clean Architecture**: Maintainable and testable code structure

## 🛠️ Technology Stack

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API framework
- **Entity Framework Core 9.0**: ORM for data access
- **PostgreSQL**: Primary database
- **JWT**: JSON Web Tokens for authentication
- **BCrypt.Net**: Password hashing library
- **Swagger/OpenAPI**: API documentation

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- IDE (Visual Studio, VS Code, or Rider)

## ⚙️ Setup & Installation

### 1. Clone the Repository
```bash
git clone <repository-url>
cd project-root
```

### 2. Database Configuration
Update the connection string in `src/Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
  }
}
```

### 3. JWT Configuration
Configure JWT settings in `src/Api/appsettings.json`:
```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-at-least-32-characters-long",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpiresInMinutes": 60
  }
}
```

### 4. Database Migration
Run the following commands to set up the database:
```bash
# Apply migrations
make update

# Or manually:
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### 5. Build and Run
```bash
# Build the project
make build

# Run the application
make run

# Or manually:
dotnet run --project src/Api
```

The API will be available at:
- HTTP: `http://localhost:5143`
- HTTPS: `https://localhost:7284` (if configured)

## 📚 API Endpoints

### Authentication

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

#### Login User
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Session Management

#### Get Current User
```http
GET /api/session
Authorization: Bearer <your-jwt-token>
```

**Response:**
```json
{
  "success": true,
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com"
}
```

## 🔧 Development Commands

The project includes a Makefile for common development tasks:

```bash
# Create a new migration
make migrate-MigrationName

# Update database
make update

# Build project
make build

# Run project
make run
```

## 🗂️ Project Structure

```
├── src/
│   ├── Api/                    # Web API layer
│   │   ├── Controllers/        # API controllers
│   │   ├── Middleware/         # Custom middleware
│   │   └── Attributes/         # Custom attributes
│   ├── Application/            # Application layer
│   │   ├── DTOs/              # Data Transfer Objects
│   │   ├── Interfaces/        # Service contracts
│   │   └── Services/          # Business logic services
│   ├── Domain/                # Domain layer
│   │   └── Entities/          # Domain entities
│   └── Infrastructure/        # Infrastructure layer
│       ├── Data/              # Database context & migrations
│       └── Repositories/      # Data access implementations
├── Makefile                   # Development commands
└── project-root.sln          # Solution file
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

### Members Table
```sql
CREATE TABLE members (
    Id SERIAL PRIMARY KEY,
    username VARCHAR(30) NOT NULL,
    email TEXT NOT NULL,
    password TEXT NOT NULL
);
```

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