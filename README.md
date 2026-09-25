# WebApplication1

A small bank-style ASP.NET Core Web API. Users can register, log in, and deposit or withdraw money from their account balance, with all balance changes recorded to an operations history table.

## Tech stack

- **.NET 10** / ASP.NET Core Web API
- **PostgreSQL** via Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **JWT authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swagger / OpenAPI** via Swashbuckle
- **xUnit + Moq** for unit testing, with EF Core InMemory and SQLite providers

## Project structure

```
WebApplication1/
├── Controllers/      # HTTP endpoints (AuthController, AccountController)
├── Services/         # Business logic (AuthService, AccountService, exceptions)
├── Data/             # AppDbContext (EF Core)
├── Models/            # Entity classes (User, Operation, OperationType)
├── DTOs/              # Request/response records (CredentialsDto, AmountDto, BalanceDto)
├── Migrations/        # EF Core migrations
├── ExceptionHandling/  # Global exception handler
└── Program.cs          # App startup and configuration

WebApplication1.Tests/
├── AuthServiceTests.cs
├── AccountServiceTests.cs
└── TestHelpers.cs
```

`AuthService` verifies credentials; `TokenService` (in `Services/`) issues JWTs on successful login.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running PostgreSQL instance

## Setup

### 1. Clone and restore

```bash
git clone <repo-url>
cd WebApplication1
dotnet restore
```

### 2. Configure the database connection

Set the PostgreSQL connection string using [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (do not commit real credentials to `appsettings.json`):

```bash
cd WebApplication1
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=bankdb;Username=youruser;Password=yourpassword"
```

Set the JWT signing key and token issuer/audience the same way (the key must be at least 32 characters):

```bash
dotnet user-secrets set "Jwt:Key" "a-long-random-secret-at-least-32-characters"
dotnet user-secrets set "Jwt:Issuer" "WebApplication1"
dotnet user-secrets set "Jwt:Audience" "WebApplication1Client"
```

### 3. Apply migrations

```bash
dotnet ef database update
```

### 4. Run the app

```bash
dotnet run
```

The API will be available at the URL printed in the console (e.g. `https://localhost:5001`). In development mode, Swagger UI is available at `/swagger`.

## Running tests

```bash
dotnet test
```

Tests cover `AuthService` and `AccountService` in isolation, using an in-memory database per test so no real database connection is required.

## API overview

### Auth (`/api/auth`)

| Method | Route | Description | Auth required |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Log in and receive a JWT | No |

**Request body** (`register` / `login`):

```json
{
  "name": "alice",
  "password": "secret123"
}
```

**Login response**:

```json
{
  "token": "eyJhbGciOi...",
  "id": 1,
  "name": "alice"
}
```

There is no `logout` endpoint: JWTs are stateless, so the server has nothing to clear. "Logging out" means the client discards the token it's holding. Include the token on later requests as:

```
Authorization: Bearer <token>
```

### Account (`/api/account`)

All endpoints require authentication (`Authorization: Bearer <token>`).

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/account/balance` | Get the current user's balance |
| POST | `/api/account/deposit` | Deposit an amount into the balance |
| POST | `/api/account/withdraw` | Withdraw an amount from the balance |

**Request body** (`deposit` / `withdraw`):

```json
{
  "amount": 50.00
}
```

**Response** (all account endpoints):

```json
{
  "balance": 150.00
}
```

## Error handling

Errors are returned in a consistent [`ProblemDetails`](https://datatracker.ietf.org/doc/html/rfc9457) shape, e.g.:

```json
{
  "status": 400,
  "title": "Недостаточно средств",
  "instance": "/api/account/withdraw"
}
```

| Status | Meaning |
| --- | --- |
| 400 | Invalid input (bad amount, failed validation) |
| 401 | Not authenticated, or invalid login credentials |
| 409 | Username already taken |
| 500 | Unexpected server error |

## Notes on the balance logic

Balance updates use a single atomic `UPDATE ... WHERE Balance + delta >= 0` statement to prevent race conditions on concurrent withdrawals, combined with a database transaction so the balance change and the corresponding history record in `Operations` are always saved together.