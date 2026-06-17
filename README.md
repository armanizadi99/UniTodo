# UniTodo API

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**UniTodo** is a modular monolith backend API for collaborative task management, built with .NET 9 using Clean Architecture and Domain-Driven Design principles. It supports reusable todo templates, active task runs with team collaboration, and automatic reset policies for recurring workflows.

---

## Key Features

### Todo Templates
- Create reusable todo list templates with items
- Support for **reset policies**: None, Daily, Weekly, Monthly
- Archive/activate templates
- Owner-based authorization

### Todo Runs (Active Instances)
- Instantiate runs from templates or create empty private runs
- **Collaboration**: Share runs with team members, assign items to members
- **Item management**: Add, delete, mark complete/incomplete, update notes, change descriptions
- **Reset logic**: Automatic reset based on policy (background job + manual trigger)
- **Visibility control**: Toggle between private and shared runs

### Authentication & Authorization
- User registration and login via ASP.NET Core Identity
- JWT Bearer token authentication
- Per-resource ownership and membership authorization

---

## Architecture

```
UniTodo (Solution)
├── UniTodo (Web API)
│   ├── Modules
│   │   ├── Auth          # Authentication module (Identity + JWT)
│   │   └── Todos         # Todo module (Templates, Runs, Items, Members)
│   │       ├── Domain    # Entities, Value Objects, Domain Events, Exceptions
│   │       ├── Application   # Services, DTOs, Interfaces (Repository, UnitOfWork)
│   │       ├── Infrastructure  # EF Core, Repositories, Configurations, Migrations
│   │       └── Api       # Controllers, Endpoint Mapping
└── UniTodo.Tests         # xUnit tests (Domain, Application, Infrastructure)
```

### Design Principles
- **Modular Monolith**: Clear separation between `Auth` and `Todos` modules
- **Domain-Driven Design**: Rich domain models (`TodoListTemplate`, `TodoListRun`, `TodoItem`, `RunMember`) with encapsulated business logic
- **Clean Architecture**: Domain → Application → Infrastructure → API layering
- **Strongly-Typed IDs**: Generic `IStronglyTypedId<T>` prevents primitive obsession
- **Result Pattern**: `Result<T>` for explicit error handling without exceptions
- **Repository & Unit of Work**: Decoupled persistence with EF Core
- **Automatic Auditing**: `IAuditable` interface with `CreatedAt`/`UpdatedAt` via `DbContext` overrides

---

## Tech Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 9.0 (ASP.NET Core) |
| Persistence | EF Core 9.0 with SQLite (separate DBs per module) |
| Identity | ASP.NET Core Identity |
| Auth | JWT Bearer Tokens |
| Testing | xUnit, NSubstitute, FluentAssertions, coverlet |
| API Docs | Swagger/OpenAPI (Swashbuckle) |
| Logging | Serilog (JSON, rolling daily) |
| Mapping | Manual DTO↔Entity mapping via extension methods |

---

## API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/register` | Register new user |
| POST | `/login` | Login and receive JWT |

### Templates (`/api/templates`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List current user's templates |
| POST | `/` | Create new template |
| GET | `/{id}` | Get template by ID |
| DELETE | `/{id}` | Delete template |
| POST | `/{templateId}/item-templates` | Add item to template |
| DELETE | `/{templateId}/template-items/{itemId}` | Remove item from template |
| GET | `/{templateId}/item-templates` | List template items |

### Runs (`/api/runs`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List current user's active runs |
| POST | `/` | Create empty private run |
| POST | `/from-template/{templateId}` | Create run from template |
| GET | `/{runId}` | Get run details |
| POST | `/{runId}/make-shared` | Make run collaborative |
| POST | `/{runId}/make-private` | Make run private |

### Run Items (`/api/runs/{runId}/items`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List items in run |
| POST | `/` | Add item to run |
| DELETE | `/{itemId}` | Delete item |
| POST | `/{itemId}/mark-complete` | Mark item complete |
| POST | `/{itemId}/mark-incomplete` | Mark item incomplete |
| POST | `/{itemId}/update-notes` | Update item notes |
| POST | `/{itemId}/change-description` | Change item description |
| POST | `/{itemId}/assign-to` | Assign item to member |

### Run Members (`/api/runs/{runId}/members`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List run members |
| POST | `/` | Add member (shared runs only) |
| DELETE | `/{userId}` | Remove member |

---

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 / JetBrains Rider / VS Code

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd UniTodo

# Restore dependencies
dotnet restore

# Apply database migrations (creates Todos.db and Auth.db)
dotnet ef database update --project UniTodo --context TodoDbContext
dotnet ef database update --project UniTodo --context AuthDbContext

# Run the API
dotnet run --project UniTodo
```

The API will be available at `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP).
Swagger UI: `https://localhost:5001/swagger`

### Configuration

Edit `UniTodo/appsettings.json`:

```json
{
  "TodoModule": {
    "ConnectionStrings": {
      "sqlite": "Data Source=Todos.db"
    }
  },
  "AuthModule": {
    "ConnectionStrings": {
      "sqlite": "Data Source=Auth.db"
    },
    "JwtSettings": {
      "Audience": "UniTodoApi",
      "Issuer": "UniTodoApi",
      "ExpirationMinutes": 1000000,
      "SecretSigningKey": "<your-secret-key-here>"
    }
  }
}
```

> **Note**: The `SecretSigningKey` must be set (e.g., via User Secrets or environment variables) for JWT signing.

---

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test coverage includes:
- Domain logic (entities, value objects, reset policies)
- Application services (template/run/item/member operations)
- Infrastructure (repositories, UserContext, UnitOfWork)

---

## Project Status

| Area | Status |
|------|--------|
| Core Domain | ✅ Complete |
| Template Management | ✅ Complete |
| Run Management | ✅ Complete |
| Collaboration (Members/Assignments) | ✅ Complete |
| Reset Policies (Daily/Weekly/Monthly) | ✅ Complete |
| Background Reset Job | ✅ Implemented (`ResetPolicyJob`) |
| Authentication (Register/Login/JWT) | ✅ Complete |
| Authorization (Owner/Member) | ✅ Complete |
| Swagger/OpenAPI Docs | ✅ Enabled |
| Unit Tests | ✅ Comprehensive |
| Integration Tests | ⏳ Planned |
| Docker Support | ⏳ Planned |

---

## License

MIT License - see [LICENSE](LICENSE) for details.

---

## Author

Developed by Hamidreza Izadi