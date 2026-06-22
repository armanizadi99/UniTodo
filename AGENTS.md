# UniTodo — Agent Guide

.NET 9 ASP.NET Core Web API (modular monolith, Clean Architecture, DDD).  
No JS/Node tooling — all commands use the `dotnet` CLI.

## Quick commands

```bash
dotnet build
dotnet test                                      # all tests
dotnet test --filter "FullyQualifiedName~Domain"  # layer filter
dotnet test --collect:"XPlat Code Coverage"       # with coverage
dotnet run --project UniTodo                      # start API (http://localhost:5000)
dotnet ef database update --project UniTodo --context TodoDbContext   # manual migration
dotnet ef database update --project UniTodo --context AuthDbContext    # manual migration
docker compose up                                 # containerized (port 8080)
```

## Architecture

- **Two modules** in `UniTodo/Modules/`:
  - `Auth/` — flat structure: `AuthDbContext` (Identity + EF), JWT Bearer auth, `JwtSettings`
  - `Todos/` — Clean Architecture: `Api/` → `Application/` → `Domain/` → `Infrastructure/`
- **Two separate SQLite databases** (`data/Todos.db`, `data/Auth.db`), auto-migrated on startup
- **Registration**: `Program.cs` calls `AddTodoModule(configSection)` and `AddAuthModule(configSection)`

## JWT secret (required before running)

The `SecretSigningKey` must be set or the app throws on startup:

```bash
# User secrets (dev)
dotnet user-secrets set "AuthModule:JwtSettings:SecretSigningKey" "your-secret-key-here" --project UniTodo

# Environment variable
$env:AuthModule__JwtSettings__SecretSigningKey = "your-secret-key-here"

# Docker (docker-compose.yml)
UNITODO_JWT_SECRET=your-secret-key-here docker compose up
```

## Testing conventions

- **xUnit** + **FluentAssertions** + **NSubstitute**, AAA pattern with `// Arrange` / `// Act` / `// Assert`
- Tests reference the main project directly (not NuGet); `InternalsVisibleTo` grants internal access
- Service tests mock `IUnitOfWork`, `ITodoListTemplateRepository`, `ITodoListRunRepository`, `IUserContext`

## Gotchas

- `Program.cs` calls `app.UseHttpsRedirection()` — the launch URL is `http://localhost:5000` (HTTPS not fully configured)
- `appsettings.json` only has `ExpirationMinutes` in `JwtSettings` — the secret comes from User Secrets / env
- `data/` dir is gitignored; SQLite DBs are created there on first run
- Serilog logs to `data/logs/` as JSON, rolling daily
- No CI workflows, no `.editorconfig`, no linter/formatter config — standard .NET conventions apply
- `Todos.txt` at repo root tracks future refactoring ideas (not user-facing docs)
