# Run History via RunId Propagation

**Status**: Draft

## Goal

When a run is reset, the new run must retain the **same `RunId` (Guid)** so that all iterations (original + resets) can be linked together. Add a use case + API endpoint to retrieve the history (closed runs) for a given `RunId`, gated by membership.

---

## 1. Entity — preserve `RunId` on reset

**File**: `UniTodo/Modules/Todos/Domain/Entities/TodoListRun.cs`

- Add a private constructor overload that accepts `Guid runId`:
  ```csharp
  private TodoListRun(string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId, Guid runId)
      : this(name, resetPolicy, isShared, ownerUserId)
  {
      RunId = runId;
  }
  ```
- In `ResetInternal()` (line 111), pass `RunId`:
  ```csharp
  var newRun = new TodoListRun(Name, ResetPolicy, IsShared, ownerId, RunId);
  ```

**No schema change** — `RunId` column already exists with no unique constraint.

---

## 2. Repository — lookup by RunId

### Interface

**File**: `UniTodo/Modules/Todos/Application/Interfaces/ITodoListRunRepository.cs`

Add:

```csharp
Task<TodoListRun?> GetTodoListRunByRunIdAsync(Guid runId, CancellationToken cancellationToken = default);
Task<IReadOnlyList<TodoListRun>> GetRunHistoryByRunIdAsync(Guid runId, CancellationToken cancellationToken = default);
```

### Implementation

**File**: `UniTodo/Modules/Todos/Infrastructure/Db/Repositories/TodoListRunRepository.cs`

- `GetTodoListRunByRunIdAsync` — includes members only (for auth check), returns any run (active or closed).
- `GetRunHistoryByRunIdAsync` — includes **items** and members, filters `Status == Closed`, orders by `CreatedAt DESC`.

---

## 3. Service — auth-gated history

**File**: `UniTodo/Modules/Todos/Application/Services/TodoListRunService.cs`

Add method:

```csharp
public async Task<Result<IReadOnlyList<TodoListRunDto>>> GetRunHistoryAsync(Guid runId, CancellationToken ct)
```

Flow:
1. Look up any run by `RunId` via `GetTodoListRunByRunIdAsync`
2. If null → `EntityNotFound`
3. If current user is not a member → `NotAuthorized`
4. Fetch closed runs via `GetRunHistoryByRunIdAsync`
5. Map to DTOs and return

---

## 4. API endpoint

**File**: `UniTodo/Modules/Todos/Api/Controllers/RunsController.cs`

```
GET api/runs/history/{runId:guid}
```

Decorated with `[Authorize]`, `[ProducesResponseType]`, XML docs, `Name = "GetRunHistory"`.

---

## 5. Tests

| Layer | File | Changes |
|-------|------|---------|
| **Domain** | `TodoListRunTests.cs` | Assert `newRun.RunId == run.RunId` in existing reset tests; add dedicated `Reset_ShouldPreserveRunId` |
| **Service** | `TodoListRunServiceTests.cs` | 4 tests: member gets history, run not found, non-member forbidden, member with empty history |
| **Repository** | `TodoListRunRepositoryTests.cs` | 4 tests: returns only closed runs with matching RunId + includes items, no match = empty, `GetTodoListRunByRunIdAsync` returns any run |
| **Background** | `ResetPolicyJobTests.cs` | Assert RunId preserved after reset |

---

## Progress

- [ ] 1. Entity — add private constructor overload, update `ResetInternal`
- [ ] 2. Repository — add and implement two new methods
- [ ] 3. Service — add `GetRunHistoryAsync` with membership check
- [ ] 4. Controller — add `GET api/runs/history/{runId:guid}` endpoint
- [ ] 5a. Domain tests — update existing + add new test
- [ ] 5b. Service tests — 4 new tests
- [ ] 5c. Repository tests — 4 new tests
- [ ] 5d. Background service tests — update to verify RunId propagation
- [ ] Build & verify — `dotnet build`, `dotnet test`
