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
docker compose up                                 # containerized (http://localhost:5000, docker maps 5000:8080)
```

## Architecture

- **Two modules** in `UniTodo/Modules/`:
  - `Auth/` — flat structure: `AuthDbContext` (Identity + EF), JWT Bearer auth, `JwtSettings`
  - `Todos/` — Clean Architecture: `Api/` → `Application/` → `Domain/` → `Infrastructure/`
- **Two separate SQL Server databases** (`UniTodo_Todos`, `UniTodo_Auth`), auto-migrated on startup
- **Registration**: `Program.cs` calls `AddTodoModule(configSection)` and `AddAuthModule(configSection)`
- **Auth Startup** (`AuthStartup.cs`): registers `AuthDbContext`, Identity, JWT Bearer auth, and `TokenService`. The `JwtSettings` is bound as a singleton.
- **Todo Startup** (`Modules/Todos/ModuleStartup/Startup.cs`): calls `AddTodoApplication()` and `AddTodoInfrastructure()`. `MapTodoEndpoints()` is a **no-op placeholder** — all Todo endpoints use `[ApiController]`-based controllers mapped via `app.MapControllers()`.

## Prerequisites

### SQL Server

A SQL Server instance is required. Use Docker (recommended):

```bash
docker compose up -d sqlserver
```

Connection strings are configured via environment variables:

```bash
$env:TodoModule__ConnectionStrings__Default = "Server=localhost,1433;Database=UniTodo_Todos;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
$env:AuthModule__ConnectionStrings__Default = "Server=localhost,1433;Database=UniTodo_Auth;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

Or via User Secrets:

```bash
dotnet user-secrets set "TodoModule:ConnectionStrings:Default" "Server=localhost,1433;Database=UniTodo_Todos;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True" --project UniTodo
dotnet user-secrets set "AuthModule:ConnectionStrings:Default" "Server=localhost,1433;Database=UniTodo_Auth;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True" --project UniTodo
```

### JWT secret

The `SecretSigningKey` must be set or the app throws on startup. The rest of the JWT config (`Audience`, `Issuer`, `ExpirationMinutes`) is in `appsettings.json`.

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
- Repository tests inherit from `RepositoryTestBase` which spins up a **SQL Server Testcontainer** — no external DB setup needed, but Docker must be running.
- Test layers mirror the Todo module: `Domain/`, `Application/`, `Infrastructure/`
- Integration tests use a `WebApplicationFactory` + SQL Server Testcontainer (`IntegrationTestsWebAppFactory`), with Respawn resetting the DB between tests; `IntegrationTestsBase` handles auth via a dummy JWT

## Gotchas

- `Program.cs` calls `app.UseHttpsRedirection()` — the launch URL is `http://localhost:5000` (HTTPS not fully configured)
- `appsettings.json` only has `ExpirationMinutes`, `Audience`, and `Issuer` in `JwtSettings` — the secret comes from User Secrets / env
- `data/` dir is gitignored (via `*.db` pattern); Serilog logs (`data/logs/`) are created there on first run
- Serilog logs to `data/logs/` as JSON, rolling daily
- No CI workflows, no `.editorconfig`, no linter/formatter config — standard .NET conventions apply
- `Todos.txt` at repo root tracks future refactoring ideas (not user-facing docs); it is gitignored by `*.txt`
- `.opencode/` and `graphify-out/` are in `.gitignore`

## decide

This project has a decision-aware planning skill at `.opencode/skills/decide/SKILL.md`. Before any planning or implementation task, read that file and follow its instructions. When you encounter a significant decision point (Level 2 or Level 3 per the skill), use the `question` tool to ask the user before proceeding.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, invoke the `skill` tool with `skill: "graphify"` before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing. (Currently no wiki/ directory exists.)
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
