# UniTodo API

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Architecture](https://img.shields.io/badge/architecture-modular%20monolith-informational.svg)](#architecture)
[![Tests](https://img.shields.io/badge/tests-xUnit-success.svg)](#testing)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](#license)

**UniTodo** is a backend API for collaborative, recurring task management. Define reusable **templates**, spin them up into live **runs**, share them with teammates, and let recurring lists reset automatically on a daily, weekly, or monthly schedule — with every past cycle preserved as history.

It is built with .NET 9 and ASP.NET Core as a **modular monolith**, following **Clean Architecture** and **Domain-Driven Design** principles.

---

## Contents

- [Concepts](#concepts)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Overview](#api-overview)
- [Testing](#testing)
- [Project Layout](#project-layout)
- [License](#license)

---

## Concepts

UniTodo is organized around three core ideas:

| Concept | Description |
|---------|-------------|
| **Template** | A reusable blueprint for a todo list (a name plus a set of item descriptions). Templates are owned by a user and can be instantiated into runs on demand. |
| **Run** | A live, working instance of a list. A run can be created empty or from a template, can be private or shared with members, and carries a **reset policy**. |
| **Iteration** | A single cycle of a run. Resetting a run closes the current iteration and opens a fresh one, carrying the items forward — so completed cycles are retained as immutable **history**. |

This iteration model is what makes recurring workflows first-class: a "Daily standup checklist" or "Weekly cleaning rota" is one run that resets on schedule, and each reset is auditable after the fact.

---

## Features

### Templates
- Create and delete reusable list templates with items
- Per-user ownership and authorization
- Instantiate a run directly from a template

### Runs
- Create private empty runs, or generate a run from a template
- **Reset policies**: `None`, `Daily`, `Weekly`, `Monthly`, with the next reset time computed automatically
- **Manual + automatic reset**: reset on demand, or let the background `ResetPolicyJob` reset due runs on schedule
- **Close** a run to make it read-only
- **History endpoint** to retrieve all closed iterations and their items
- Toggle **shared / private** visibility

### Collaboration
- Share a run and add or remove members
- Assign individual items to specific members
- Members can act on items assigned to them; owners retain full control

### Item lifecycle
- Add, delete, and re-describe items
- Mark complete / incomplete
- Attach and update free-text notes
- Assign / unassign to a member

### Authentication & Authorization
- Registration and login via ASP.NET Core Identity
- Stateless **JWT Bearer** authentication
- Per-resource ownership and membership checks enforced in the domain layer

---

## Architecture

UniTodo is a **modular monolith** — a single deployable that maintains strict internal boundaries between modules.

```
UniTodo.sln
├── UniTodo/                     # ASP.NET Core Web API host
│   └── Modules/
│       ├── Auth/                # Identity + JWT (flat module)
│       │   ├── Controllers/     #   register / login
│       │   ├── DB/              #   AuthDbContext, ApplicationUser, migrations
│       │   └── Services/        #   JWT token creation
│       └── Todos/               # Core domain (Clean Architecture)
│           ├── Api/             #   Controllers + endpoint mapping
│           ├── Application/     #   Services, DTOs, interfaces, background jobs
│           ├── Domain/          #   Entities, value objects, enums, Result
│           └── Infrastructure/  #   EF Core, repositories, configurations, migrations
└── UniTodo.Tests/               # xUnit tests (Domain / Application / Infrastructure)
```

Each module owns its **own SQLite database** (`Auth.db`, `Todos.db`), keeping persistence concerns isolated. Modules register themselves through `AddAuthModule(...)` and `AddTodoModule(...)` in `Program.cs`.

### Design principles

- **Clean Architecture** — dependencies point inward: `Api → Application → Domain`, with `Infrastructure` plugging in behind interfaces.
- **Domain-Driven Design** — `Run` is an aggregate root that encapsulates iterations, members, and all invariants (authorization, reset rules, item state). Business logic lives in the domain, not the controllers.
- **Result pattern** — operations return `Result` / `Result<T>` with typed `DomainError`s instead of throwing for expected failures; controllers translate errors to the right HTTP status.
- **Strongly-typed IDs & value objects** — `UserId`, `TodoItemDescription`, and `TodoItemNotes` prevent primitive obsession and centralize validation.
- **Repository + Unit of Work** — persistence is decoupled behind `IRepository`, `IRunRepository`, `ITodoListTemplateRepository`, and `IUnitOfWork`.
- **Automatic auditing** — entities implementing `IAuditable` get `CreatedAt` / `UpdatedAt` stamped via `DbContext` overrides.

---

## Tech Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 9.0 / ASP.NET Core |
| Persistence | EF Core 9 with SQLite (one database per module) |
| Identity | ASP.NET Core Identity |
| Auth | JWT Bearer tokens |
| API docs | Swagger / OpenAPI (Swashbuckle) |
| Logging | Serilog (structured JSON, rolling daily) |
| Testing | xUnit, NSubstitute, FluentAssertions, coverlet |
| Containerization | Docker + Docker Compose |

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- (Optional) Docker, for containerized runs
- An IDE such as Visual Studio 2022, JetBrains Rider, or VS Code

### Run locally

```bash
# 1. Clone and restore
git clone <repository-url>
cd UniTodo
dotnet restore

# 2. Set the required JWT signing key (see Configuration)
dotnet user-secrets set "AuthModule:JwtSettings:SecretSigningKey" "your-long-random-secret" --project UniTodo

# 3. Run (migrations apply automatically on startup)
dotnet run --project UniTodo
```

The API starts at **http://localhost:5000**, with interactive Swagger UI at **http://localhost:5000/swagger**.

### Run with Docker

```bash
# Provide the JWT secret via env var, then start
export UNITODO_JWT_SECRET="your-long-random-secret"   # PowerShell: $env:UNITODO_JWT_SECRET = "..."
docker compose up
```

The containerized API listens on **http://localhost:8080** and persists its SQLite databases to the `./unitodo-data` volume.

---

## Configuration

The JWT **`SecretSigningKey` is required** — the application fails fast on startup if it is missing. Provide it through any standard ASP.NET Core configuration source:

| Environment | How |
|-------------|-----|
| Local dev | `dotnet user-secrets set "AuthModule:JwtSettings:SecretSigningKey" "..."` |
| Any host | Environment variable `AuthModule__JwtSettings__SecretSigningKey` |
| Docker Compose | Environment variable `UNITODO_JWT_SECRET` (mapped in `docker-compose.yml`) |

Non-secret JWT settings (e.g. `ExpirationMinutes`) live in `appsettings.json`. Serilog writes structured logs to `data/logs/` as daily rolling JSON files.

---

## API Overview

All Todos endpoints require a valid `Authorization: Bearer <token>` header. Obtain a token via `POST /api/auth/login`.

### Auth
| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/auth/register` | Create a new user account |
| `POST` | `/api/auth/login` | Authenticate and receive a JWT |

### Templates
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/templates` | List the current user's templates |
| `POST` | `/api/templates` | Create a template |
| `GET` | `/api/templates/{id}` | Get a template by id |
| `DELETE` | `/api/templates/{id}` | Delete a template |

> Template items are managed via the `TemplateItems` endpoints under a template.

### Runs
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/runs` | List the current user's active runs |
| `POST` | `/api/runs` | Create a private empty run |
| `POST` | `/api/runs/from-template/{templateId}` | Create a run from a template |
| `GET` | `/api/runs/{runId}` | Get a run by id |
| `POST` | `/api/runs/{runId}/make-shared` | Make a run shared |
| `POST` | `/api/runs/{runId}/make-private` | Make a run private |
| `POST` | `/api/runs/{runId}/close` | Close the run (read-only) |
| `POST` | `/api/runs/{runId}/reset` | Close the current iteration and open a new one |
| `POST` | `/api/runs/{runId}/reset-policy` | Update the run's reset policy |
| `GET` | `/api/runs/{runId}/history` | List closed iterations and their items |

### Run items
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/runs/{runId}/items` | List items in the current iteration |
| `POST` | `/api/runs/{runId}/items` | Add an item |
| `DELETE` | `/api/runs/{runId}/items/{itemId}` | Delete an item |
| `POST` | `/api/runs/{runId}/items/{itemId}/mark-complete` | Mark complete |
| `POST` | `/api/runs/{runId}/items/{itemId}/mark-incomplete` | Mark incomplete |
| `POST` | `/api/runs/{runId}/items/{itemId}/update-notes` | Update notes |
| `POST` | `/api/runs/{runId}/items/{itemId}/change-description` | Change description |
| `POST` | `/api/runs/{runId}/items/{itemId}/assign-to` | Assign to a member |

### Run members
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/runs/{runId}/members` | List members |
| `POST` | `/api/runs/{runId}/members` | Add a member |
| `DELETE` | `/api/runs/{runId}/members/{userId}` | Remove a member |

For full request/response schemas, see the **Swagger UI** at `/swagger`.

---

## Testing

```bash
# Run the full suite
dotnet test

# Filter by layer
dotnet test --filter "FullyQualifiedName~Domain"

# Collect coverage
dotnet test --collect:"XPlat Code Coverage"
```

Tests follow the **Arrange / Act / Assert** convention using xUnit, FluentAssertions, and NSubstitute. The test project references the API project directly (`InternalsVisibleTo`), and service tests mock `IUnitOfWork`, the repositories, and `IUserContext`. Coverage spans:

- **Domain** — entity invariants, value objects, and reset logic
- **Application** — template, run, item, and member services
- **Infrastructure** — repositories, `UserContext`, and `UnitOfWork`

---

## Project Layout

```
UniTodo/
├── UniTodo/                 # Web API host + Auth & Todos modules
├── UniTodo.Tests/           # xUnit test project
├── docker-compose.yml       # Containerized run (port 8080)
├── Dockerfile               # Multi-stage .NET build
└── UniTodo.sln
```

---

## License

Released under the **MIT License**. See [LICENSE](LICENSE) for details.

---

## Author

Developed by **Hamidreza Izadi**.
