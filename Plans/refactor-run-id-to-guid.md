# [COMPLETED] Refactor: Use `Guid RunId` for All Run Operations

## Context

- `TodoListRun` has **two** identifiers: `int Id` (auto-increment PK, changes on each reset) and `Guid RunId` (stable business identifier, same across all iterations of a logical run)
- Currently **every** operation uses `int Id` — the `Guid RunId` is stored and returned in DTOs but never queried
- Goal: all run-lookup operations use `Guid RunId` instead of `int Id`

## Key Design Decisions

| Decision | Choice |
|---|---|
| `RunMember.RunId` | Rename to `TodoListRunId`, change type from `int` → `Guid` (members belong to the logical run, not a specific iteration) |
| `TodoItem.RunId` | Keep as `int` FK to `TodoListRun.Id` (items are per-iteration for history) |
| `RunId` index | Non-unique index |
| `TodoListRunDto.Id` | Keep in DTO — needed for history/deletion of specific iterations |
| `ResetInternal()` | Preserve `RunId` across iterations so all iterations of a logical run share the same Guid |

## Step-by-step Changes

### Step 1 — Domain: Preserve `RunId` across resets

**File:** `UniTodo/Modules/Todos/Domain/Entities/TodoListRun.cs`

- Add constructor overload that accepts `Guid runId`:
  ```csharp
  public TodoListRun(string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId, Guid runId)
      : this(name, resetPolicy, isShared, ownerUserId)
  {
      RunId = runId;
  }
  ```
- In `ResetInternal()`, pass `RunId` to the new run:
  ```csharp
  var newRun = new TodoListRun(Name, ResetPolicy, IsShared, ownerId, RunId);
  ```

### Step 2 — Domain: Rename `RunMember.RunId` → `TodoListRunId` (int → Guid)

**File:** `UniTodo/Modules/Todos/Domain/Entities/RunMember.cs`

```csharp
public Guid TodoListRunId { get; private set; }  // was: int RunId
```

### Step 3 — Repository Interface

**File:** `UniTodo/Modules/Todos/Application/Interfaces/ITodoListRunRepository.cs`

- Rename both overloads:
  - `GetTodoListRunByIdAsync(int id, bool includeItems, ...)` → `GetTodoListRunByRunIdAsync(Guid runId, bool includeItems, ...)`
  - `GetTodoListRunByIdAsync(int id, int itemId, ...)` → `GetTodoListRunByRunIdAsync(Guid runId, int itemId, ...)`
- `GetUserActiveRunsAsync` and `GetRunsDueForResetAsync` unchanged

### Step 4 — Repository Implementation

**File:** `UniTodo/Modules/Todos/Infrastructure/Db/Repositories/TodoListRunRepository.cs`

- Change queries from `e.Id == id` → `e.RunId == runId`
- Update method signatures to match interface

### Step 5 — EF Configurations

**File:** `UniTodo/Modules/Todos/Infrastructure/Db/Configurations/TodoListRunConfiguration.cs`

- Add non-unique index:
  ```csharp
  builder.HasIndex(e => e.RunId).IsUnique(false);
  ```
- Change Members FK to point to `TodoListRun.RunId`:
  ```csharp
  builder.HasMany(e => e.Members)
      .WithOne(e => e.Run)
      .HasForeignKey(e => e.TodoListRunId)
      .HasPrincipalKey(e => e.RunId);
  ```
- Leave TodoItems FK unchanged (still points to `int Id` PK)

**File:** `UniTodo/Modules/Todos/Infrastructure/Db/Configurations/RunMemberConfiguration.cs`

- Update composite key:
  ```csharp
  builder.HasKey(e => new { e.UserId, e.TodoListRunId });  // was: e.RunId
  ```

**Migration:** Generate a new migration for the index and FK changes.

### Step 6 — Services

**File:** `UniTodo/Modules/Todos/Application/Services/TodoListRunService.cs`

- `MakeTodoListRunSharedAsync`: `int id` → `Guid runId`
- `MakeTodoListRunPrivateAsync`: `int id` → `Guid runId`
- Add new method for the controller:
  ```csharp
  public async Task<Result<TodoListRunDto>> GetTodoListRunByRunIdAsync(Guid runId, CancellationToken ct)
  ```
- All repository calls: `GetTodoListRunByIdAsync(...)` → `GetTodoListRunByRunIdAsync(...)`

**File:** `UniTodo/Modules/Todos/Application/Services/TodoListRunItemsService.cs`

- All 8 methods: `int todoListRunId` → `Guid runId`
- Repository calls: same pattern as above

**File:** `UniTodo/Modules/Todos/Application/Services/TodoListRunMembersService.cs`

- All 3 methods: `int todoListRunId` → `Guid runId`
- Repository calls: same pattern

### Step 7 — Controllers

**File:** `UniTodo/Modules/Todos/Api/Controllers/RunsController.cs`

- Route constraints: `{runId:int:min(1)}` → `{runId:guid}` on all routes
- Parameters: `[FromRoute] int runId` → `[FromRoute] Guid runId`
- Route name: `"GetRunById"` → `"GetRunByRunId"`
- `CreatedAtRoute` uses `result.Value.RunId`
- Implement `GetRunByIdAsync` stub to call the new service method

**File:** `UniTodo/Modules/Todos/Api/Controllers/RunItemsController.cs`

- Route template: `{runId:int:min(1)}` → `{runId:guid}`
- Parameters: `[FromRoute] int runId` → `[FromRoute] Guid runId`

**File:** `UniTodo/Modules/Todos/Api/Controllers/RunMembersController.cs`

- Route template: `{runId:int:min(1)}` → `{runId:guid}`
- Parameters: `[FromRoute] int runId` → `[FromRoute] Guid runId`

### Step 8 — Tests

**File:** `UniTodo.Tests/TodoModuleTests/Application/TodoListRunServiceTests.cs`
- `_runRepository.GetTodoListRunByIdAsync(1, ...)` → `GetTodoListRunByRunIdAsync(Arg.Any<Guid>(), ...)`
- `MakeTodoListRunSharedAsync(1, ...)` → `MakeTodoListRunSharedAsync(run.RunId, ...)`

**File:** `UniTodo.Tests/TodoModuleTests/Application/TodoListRunItemsServiceTests.cs`
- Same pattern: magic number `1` → `run.RunId`

**File:** `UniTodo.Tests/TodoModuleTests/Application/TodoListRunMembersServiceTests.cs`
- Same pattern

**File:** `UniTodo.Tests/TodoModuleTests/Infrastructure/Db/Repositories/TodoListRunRepositoryTests.cs`
- `GetTodoListRunByIdAsync(run.Id, ...)` → `GetTodoListRunByRunIdAsync(run.RunId, ...)`

**File:** `UniTodo.Tests/TodoModuleTests/Application/ResetPolicyJobTests.cs`
- No changes needed

## Full File Change List

| Layer | File | Change |
|---|---|---|
| Domain | `TodoListRun.cs` | Preserve `RunId` on reset, add constructor overload |
| Domain | `RunMember.cs` | Rename `RunId` → `TodoListRunId`, `int` → `Guid` |
| Application/Interfaces | `ITodoListRunRepository.cs` | Method sigs use `Guid runId` |
| Application/Services | `TodoListRunService.cs` | Params `int`→`Guid`, add `GetByRunIdAsync` |
| Application/Services | `TodoListRunItemsService.cs` | Params `int`→`Guid` |
| Application/Services | `TodoListRunMembersService.cs` | Params `int`→`Guid` |
| API/Controllers | `RunsController.cs` | Route constraints, params, implement stub |
| API/Controllers | `RunItemsController.cs` | Route constraints, params |
| API/Controllers | `RunMembersController.cs` | Route constraints, params |
| Infrastructure/Repos | `TodoListRunRepository.cs` | Query by `RunId` |
| Infrastructure/Config | `TodoListRunConfiguration.cs` | Index + FK change for Members |
| Infrastructure/Config | `RunMemberConfiguration.cs` | Composite key update |
| Infrastructure/Migrations | New migration | Index + FK schema changes |
| Tests | `TodoListRunServiceTests.cs` | Mocks/assertions use `RunId` |
| Tests | `TodoListRunItemsServiceTests.cs` | Mocks/assertions use `RunId` |
| Tests | `TodoListRunMembersServiceTests.cs` | Mocks/assertions use `RunId` |
| Tests | `TodoListRunRepositoryTests.cs` | Query by `RunId` |
