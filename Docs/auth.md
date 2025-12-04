
# Auth Operations

## User Table

```sql
CREATE TABLE IF NOT EXISTS members (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(30) NOT NULL,
    email VARCHAR(255) NOT NULL,
    password VARCHAR(255) NOT NULL
);
```

## Registration
- Users register by sending a POST request to the `/auth/register` endpoint.
- Required fields: username, email, password

## Login
- Users log in by sending a POST request to the `/auth/login` endpoint.
- Returns a JWT token.

## JWT Token
- The token is sent in the Authorization header: `Authorization: Bearer <token>`

## Getting Started
1. Create the table in your database using the SQL above.
2. Update the connection string in your `appsettings.yml` file.
3. Start the project: `dotnet run`

---
For more details, check the code and controller files.
