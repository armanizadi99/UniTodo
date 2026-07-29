# Plan: Switch SQLite → SQL Server

## Overview

Switch the UniTodo database provider from SQLite to SQL Server, using Docker for running SQL Server in development and Testcontainers for repository tests.

## Decisions (confirmed via decide skill)

| Decision | Choice |
|----------|--------|
| SQL Server variant | Docker (`mcr.microsoft.com/mssql/server:2022-latest`) |
| Connection string | User secrets / env var for dev, env vars for Docker |
| Migration strategy | Delete old SQLite migrations, generate fresh for SQL Server |
| Test strategy | SQL Server Testcontainers for repository tests |
| RunRepository raw SQL | Refactor `FromSqlInterpolated` to LINQ |

---

## Step-by-step

### 1. NuGet packages

**`UniTodo/UniTodo.csproj`**
- Remove: `Microsoft.EntityFrameworkCore.Sqlite`
- Add: `Microsoft.EntityFrameworkCore.SqlServer`

**`UniTodo.Tests/UniTodo.Tests.csproj`**
- Add: `Testcontainers.MsSql`

### 2. Connection strings

**`UniTodo/appsettings.json`** — rename both connection string keys from `"sqlite"` to `"Default"` and add SQL Server placeholder:

```json
"TodoModule": {
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=UniTodo_Todos;User Id=sa;Password=<set-via-env>;TrustServerCertificate=True"
  }
},
"AuthModule": {
  "ConnectionStrings": {
    "Default": "Server=localhost,1433;Database=UniTodo_Auth;User Id=sa;Password=<set-via-env>;TrustServerCertificate=True"
  },
  "JwtSettings": { ... }
}
```

Password set via environment variable: `AuthModule__ConnectionStrings__Default` / `TodoModule__ConnectionStrings__Default`.

### 3. DbContext DI registrations

**`UniTodo/Modules/Auth/AuthStartup.cs`** (line ~20):
```csharp
// Before: options.UseSqlite(...)
options.UseSqlServer(
    moduleConfiguration.GetConnectionString("Default"),
    opts => opts.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));
```

**`UniTodo/Modules/Todos/Infrastructure/DependencyInjection.cs`** (line ~17):
```csharp
// Before: options.UseSqlite(...)
options.UseSqlServer(moduleConfiguration.GetConnectionString("Default"));
```

### 4. Migrations

Delete these 6 files:

1. `UniTodo/Modules/Auth/DB/Migrations/20260703102526_InitialAuth.cs`
2. `UniTodo/Modules/Auth/DB/Migrations/20260703102526_InitialAuth.Designer.cs`
3. `UniTodo/Modules/Auth/DB/Migrations/AuthDbContextModelSnapshot.cs`
4. `UniTodo/Modules/Todos/Infrastructure/Db/Migrations/20260708082359_InitialTodo.cs`
5. `UniTodo/Modules/Todos/Infrastructure/Db/Migrations/20260708082359_InitialTodo.Designer.cs`
6. `UniTodo/Modules/Todos/Infrastructure/Db/Migrations/TodoDbContextModelSnapshot.cs`

Generate fresh migrations:

```bash
dotnet ef migrations add InitialAuth --project UniTodo --context AuthDbContext --output-dir Modules/Auth/DB/Migrations
dotnet ef migrations add InitialTodo --project UniTodo --context TodoDbContext --output-dir Modules/Todos/Infrastructure/Db/Migrations
```

### 5. Refactor RunRepository (raw SQL → LINQ)

**`UniTodo/Modules/Todos/Infrastructure/Db/Repositories/RunRepository.cs`**

Replace `FromSqlInterpolated` in `GetRunsDueForResetAsync` with pure LINQ:

```csharp
async Task<IReadOnlyList<Run>> IRunRepository.GetRunsDueForResetAsync(CancellationToken cancellationToken)
{
    var now = DateTimeOffset.UtcNow;
    return await _dbSet
        .Where(r => r.Status == TodoListRunStatus.Active
                 && r.ResetPolicy != ResetPolicy.None
                 && r.ResetsAt != null
                 && r.ResetsAt <= now)
        .Include(r => r.Iterations.OrderByDescending(i => i.Id).Take(1))
            .ThenInclude(i => i.RunItems)
        .Include(r => r.Members)
        .AsSplitQuery()
        .ToListAsync(cancellationToken);
}
```

Also update/remove SQLite-specific comments (lines 26-27, 73-75). `AsSplitQuery()` works on SQL Server — only the comments need updating.

### 6. Test infrastructure — Testcontainers

**`UniTodo.Tests/TodoModuleTests/Infrastructure/Db/RepositoryTestBase.cs`**

Rewrite to use `Testcontainers.MsSql`:

```csharp
using Testcontainers.MsSql;
using Microsoft.EntityFrameworkCore;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db;

public abstract class RepositoryTestBase : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    protected TodoDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
        Context = new TodoDbContext(options);
        Context.Database.EnsureCreated();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }
}
```

Update `CreateNewContext()` helpers in both `TodoListTemplateRepositoryTests.cs` and `RunRepositoryTests.cs` to use `UseSqlServer` with the shared container's connection string.

Remove `using Microsoft.Data.Sqlite` from test files.

### 7. Docker Compose

**`docker-compose.yml`** — add SQL Server service:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: unitodo-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=${SQL_SERVER_PASSWORD:-YourStrong!Passw0rd}
    ports:
      - "1433:1433"
    volumes:
      - ./sqlserver-data:/var/opt/mssql/data
    restart: unless-stopped

  unitodo:
    build:
      context: .
      dockerfile: Dockerfile
    image: unitodo:latest
    container_name: unitodo-app
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - AuthModule__JwtSettings__SecretSigningKey=${UNITODO_JWT_SECRET:-ThisIsAPlaceholderSecretChangeItInProduction}
      - AuthModule__ConnectionStrings__Default=Server=sqlserver,1433;Database=UniTodo_Auth;User Id=sa;Password=${SQL_SERVER_PASSWORD:-YourStrong!Passw0rd};TrustServerCertificate=True
      - TodoModule__ConnectionStrings__Default=Server=sqlserver,1433;Database=UniTodo_Todos;User Id=sa;Password=${SQL_SERVER_PASSWORD:-YourStrong!Passw0rd};TrustServerCertificate=True
    depends_on:
      - sqlserver
    restart: unless-stopped
```

### 8. Cleanup

- **`AGENTS.md`**: Update `dotnet ef database update` commands (same syntax, no SQLite-specific flags)
- **`appsettings.Development.json`**: Remove any SQLite-specific overrides if present
- **`plans/` directory**: Add `.gitkeep` or update `.gitignore` to keep this directory
- Remove `data/` volume mapping from docker-compose if no longer needed

---

## Files modified

| # | File | Action |
|---|------|--------|
| 1 | `UniTodo/UniTodo.csproj` | Edit |
| 2 | `UniTodo.Tests/UniTodo.Tests.csproj` | Edit |
| 3 | `UniTodo/appsettings.json` | Edit |
| 4 | `UniTodo/Modules/Auth/AuthStartup.cs` | Edit |
| 5 | `UniTodo/Modules/Todos/Infrastructure/DependencyInjection.cs` | Edit |
| 6 | `UniTodo/Modules/Todos/Infrastructure/Db/Repositories/RunRepository.cs` | Edit |
| 7 | `UniTodo.Tests/TodoModuleTests/Infrastructure/Db/RepositoryTestBase.cs` | Edit |
| 8 | `UniTodo.Tests/TodoModuleTests/Infrastructure/Db/Repositories/TodoListTemplateRepositoryTests.cs` | Edit |
| 9 | `UniTodo.Tests/TodoModuleTests/Infrastructure/Db/Repositories/RunRepositoryTests.cs` | Edit |
| 10 | `docker-compose.yml` | Edit |
| 11–16 | 6 existing migration files | Delete |
| 17–22 | 6 new migration files | Create |
| 23 | `AGENTS.md` | Edit |