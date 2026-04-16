# RecipeShare Backend

Backend API for RecipeShare — a social platform for sharing, discovering, and organizing recipes. Built with ASP.NET Core 8 following Clean Architecture principles.

## Tech Stack

- **Framework:** ASP.NET Core 8.0
- **ORM:** Entity Framework Core 8.0 (Code First)
- **Database:** PostgreSQL
- **Authentication:** JWT (JSON Web Tokens)
- **Password Hashing:** ASP.NET Identity PasswordHasher (PBKDF2)
- **Object Mapping:** Mapster
- **Image Storage:** Cloudinary
- **Push Notifications:** Firebase Cloud Messaging
- **API Documentation:** Swagger / Swashbuckle

## Architecture

The project follows **Clean Architecture** with 4 layers. Dependencies always point inward — outer layers depend on inner layers, never the reverse.

```
┌──────────────────────────────────────────┐
│  API (Controllers, Middleware)            │  ← outermost
├──────────────────────────────────────────┤
│  Infrastructure (EF Core, Repositories,  │
│  JWT, Cloudinary, Firebase)              │
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
│   ├── Enums/                           # ImageFolder enum + extensions
│   ├── Exceptions/                      # Custom exceptions (→ HTTP status codes)
│   ├── Interfaces/
│   │   ├── Repositories/                # Repository + UnitOfWork interfaces
│   │   └── Services/                    # Service interfaces
│   ├── Mappings/                        # Mapster configuration
│   └── Services/                        # Service implementations
├── RecipeShare.Infrastructure/          # Data access and external services
│   ├── Auth/                            # JWT token generation
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/              # EF Core Fluent API configurations
│   │   └── Migrations/
│   ├── Repositories/                    # Repository + UnitOfWork implementations
│   ├── Storage/                         # Cloudinary image storage
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
    "Secret": "your-256-bit-secret-key-here-minimum-32-characters!!",
    "Issuer": "RecipeShare",
    "Audience": "RecipeShareApp",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "RootFolder": "recipeshare"
  }
}
```

Get Cloudinary credentials from your [Cloudinary Dashboard](https://console.cloudinary.com/).

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

Protected endpoints require the `Authorization: Bearer {token}` header.

### Authentication

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Log in and receive JWT + refresh token | No |
| POST | `/api/auth/refresh` | Exchange refresh token for new token pair | No |
| POST | `/api/auth/logout` | Revoke a refresh token | Yes |

### User Profile

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/user` | Get my profile | Yes |
| GET | `/api/user/{id}` | Get user profile by ID | No |
| PUT | `/api/user` | Update my profile | Yes |
| PUT | `/api/user/password` | Change my password | Yes |
| PUT | `/api/user/email` | Change my email | Yes |
| PUT | `/api/user/image` | Upload/replace profile image (multipart/form-data) | Yes |
| DELETE | `/api/user/image` | Delete my profile image | Yes |

### Recipes

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/recipes` | Get paginated recipe feed (cursor-based, supports filters) | Yes |
| GET | `/api/recipes/{id}` | Get recipe detail | Yes |
| POST | `/api/recipes` | Create a new recipe | Yes |
| PUT | `/api/recipes/{id}` | Update a recipe | Yes |
| DELETE | `/api/recipes/{id}` | Delete a recipe (admin can delete any) | Yes |
| PUT | `/api/recipes/{id}/image` | Upload/replace recipe image (multipart/form-data) | Yes |
| DELETE | `/api/recipes/{id}/image` | Delete recipe image | Yes |

**Query parameters for `GET /api/recipes`:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `search` | string | Filter by title |
| `categoryId` | int | Filter by category |
| `tagIds` | int[] | Filter by tags |
| `difficulty` | string | Filter by difficulty (`Easy`, `Medium`, `Hard`) |
| `cursor` | int | ID of the last seen recipe (for pagination) |
| `pageSize` | int | Number of results per page (default: 10) |

### Categories

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/categories` | Get all active categories | No |
| GET | `/api/categories/{id}` | Get category by ID | No |

### Tags

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/tags` | Get all active tags | No |
| GET | `/api/tags/{id}` | Get tag by ID | No |

### Lookups

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/lookups/measurement-units` | Get all available measurement units | No |
| GET | `/api/lookups/difficulty-levels` | Get all available difficulty levels | No |

### Admin — Categories

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/admin/categories` | Get all categories (including inactive) | Admin |
| GET | `/api/admin/categories/{id}` | Get category by ID | Admin |
| POST | `/api/admin/categories` | Create a new category | Admin |
| PUT | `/api/admin/categories/{id}` | Update a category | Admin |
| DELETE | `/api/admin/categories/{id}` | Delete a category | Admin |
| PATCH | `/api/admin/categories/{id}/toggle-IsActive` | Toggle category active status | Admin |

### Admin — Tags

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/admin/tags` | Get all tags (including inactive) | Admin |
| GET | `/api/admin/tags/{id}` | Get tag by ID | Admin |
| POST | `/api/admin/tags` | Create a new tag | Admin |
| PUT | `/api/admin/tags/{id}` | Update a tag | Admin |
| DELETE | `/api/admin/tags/{id}` | Delete a tag | Admin |
| PATCH | `/api/admin/tags/{id}/toggle-IsActive` | Toggle tag active status | Admin |

### Admin — Users

| Method | Route | Description | Auth Required |
|--------|-------|-------------|---------------|
| GET | `/api/admin/users/search` | Search users by name (paginated) | Admin |
