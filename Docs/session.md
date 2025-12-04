
# Session Operations

## Session Logic
- After the user logs in, authentication is done via JWT token.
- Once the token is validated, the user's information is stored in `HttpContext.Items["Member"]`.

## Session Endpoints
- In endpoints like `/session`, user information is validated using JWT.

## SQL Code
Sessions are managed via the `members` table. There is no separate session table.

## Getting Started
1. Make sure the `members` table exists in your database.
2. Start the project: `dotnet run`
3. Send requests to endpoints using a JWT token.

---
For more details, check the middleware and controller files.
