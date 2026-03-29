# RecipeShare Backend

Backend API for RecipeShare — a social platform for sharing, discovering, and organizing recipes. Built with ASP.NET Core 8 following Clean Architecture principles.

## Tech Stack

- **Framework:** ASP.NET Core 8.0
- **ORM:** Entity Framework Core 8.0 (Code First)
- **Database:** PostgreSQL
- **Authentication:** JWT (JSON Web Tokens)
- **Password Hashing:** ASP.NET Identity PasswordHasher (PBKDF2)
- **Object Mapping:** Mapster
- **Image Storage:** AWS S3
- **Push Notifications:** Firebase Cloud Messaging
- **API Documentation:** Swagger / Swashbuckle

## Architecture

The project follows **Clean Architecture** with 4 layers. Dependencies always point inward — outer layers depend on inner layers, never the reverse.

```
┌──────────────────────────────────────────┐
│  API (Controllers, Middleware)            │  ← outermost
├──────────────────────────────────────────┤
│  Infrastructure (EF Core, Repositories,  │
│  JWT, S3, Firebase)                      │
├──────────────────────────────────────────┤
│  Application (Services, DTOs,            │
│  Interfaces, Exceptions)                 │
├──────────────────────────────────────────┤
│  Domain (Entities, Enums)                │  ← innermost
└──────────────────────────────────────────┘
```

**Dependency flow:**

- **Domain** → nothing (zero external dependencies)
- **Application** → Domain
- **Infrastructure** → Domain + Application
- **API** → Application + Infrastructure (Infrastructure only for DI registration)

**Request flow:**

```
HTTP Request → Controller → Service → Repository → Database
                                    → UnitOfWork.SaveChangesAsync()
             ← DTO Response
```

## Project Structure

```
RecipeShare/
├── RecipeShare.sln
├── RecipeShare.Domain/                  # Entities and Enums
│   ├── Entities/
│   └── Enums/
├── RecipeShare.Application/             # Business logic
│   ├── DTOs/                            # Request/Response objects
│   ├── Exceptions/                      # Custom exceptions (→ HTTP status codes)
│   ├── Interfaces/
│   │   ├── Repositories/                # Repository + UnitOfWork interfaces
│   │   └── Services/                    # Service interfaces
│   └── Services/                        # Service implementations
├── RecipeShare.Infrastructure/          # Data access and external services
│   ├── Auth/                            # JWT token generation
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/              # EF Core Fluent API configurations
│   │   └── Migrations/
│   ├── Repositories/                    # Repository + UnitOfWork implementations
│   └── DependencyInjection.cs           # DI registration for all services
└── RecipeShare.API/                     # Entry point
    ├── Controllers/
    ├── Extensions/                      # ClaimsPrincipal extension methods
    ├── Middleware/                       # Exception handling middleware
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json     # Local config (gitignored)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [PostgreSQL](https://www.postgresql.org/download/) (local or cloud-hosted, e.g. [Neon](https://neon.tech))
- [EF Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Setup

### 1. Clone the repository

```bash
git clone https://github.com/kristijan-gaspar/recipeshare-backend.git
cd recipeshare-backend
```

### 2. Configure the database connection

Create `RecipeShare.API/appsettings.Development.json` (this file is gitignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=recipeshare;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here-minimum-32-characters!!"
  }
}
```

The `Jwt.Issuer`, `Jwt.Audience`, and `Jwt.ExpirationMinutes` values are already set in `appsettings.json` — only override them here if needed.

### 3. Apply database migrations

```bash
dotnet ef database update -p RecipeShare.Infrastructure -s RecipeShare.API
```

### 4. Run the project

```bash
dotnet run --project RecipeShare.API
```

The API will start at:
- **HTTP:** http://localhost:5285
- **HTTPS:** https://localhost:7223

Swagger UI is available at http://localhost:5285/swagger.

## API Endpoints

### Authentication

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Log in and receive JWT token | No |

Protected endpoints require the `Authorization: Bearer {token}` header.
