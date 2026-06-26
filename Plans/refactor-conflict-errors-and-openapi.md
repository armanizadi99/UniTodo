# Refactor: Domain Conflict Errors + Cleaner OpenAPI Documentation

## Motivation

All domain failures that are not `NotFound`, `NotAuthorized`, or `DuplicateEntities` currently use `DomainError.InvalidOperation` which maps to **400 Bad Request**. Many of these are semantically **state conflicts** — the request is well-formed and the operation is valid, but it conflicts with the current state of the aggregate/entity.

**Example:** `POST /api/runs/1/make-shared` on a run that is already shared returns 400 "Bad Request". The correct status is **409 Conflict** (RFC 7231 §6.5.8).

The OpenAPI spec also needs updating to document all 409 responses consistently.

---

## Plan

### Step 1 — Add `Conflict` to `DomainErrorCodes`

**File:** `UniTodo/Modules/Todos/Domain/Common/DomainErrorCodes.cs`

Add a new `Conflict` enum member.

```csharp
public enum DomainErrorCodes
{
    EntityNotFound,
    NotAuthorized,
    InvalidOperation,
    DuplicateEntities,
    Conflict,          // new
}
```

### Step 2 — Add `DomainError.Conflict()` factory

**File:** `UniTodo/Modules/Todos/Domain/Common/DomainError.cs`

```csharp
public static DomainError Conflict(string message)
{
    return new DomainError(DomainErrorCodes.Conflict, message);
}
```

### Step 3 — Map `Conflict` to 409 in error extension

**File:** `UniTodo/Modules/Todos/Api/Extensions/DomainErrorExtensions.cs`

Add a branch for `DomainErrorCodes.Conflict`. Since `DuplicateEntities` already returns the same `ConflictObjectResult` shape, merge them with a pattern match:

```csharp
DomainErrorCodes.DuplicateEntities or DomainErrorCodes.Conflict
    => new ConflictObjectResult(new ProblemDetails
    {
        Detail = error.Message,
        Status = StatusCodes.Status409Conflict,
        Title = "Conflict",
        Type = "https://httpstatuses.com/409"
    }),
```

### Step 4 — Change 20 `InvalidOperation` calls to `Conflict`

#### `TodoListTemplate.cs` (2 changes)

| Line | Current | New |
|------|---------|-----|
| 35 | `InvalidOperation("This todo list is already archived.")` | `Conflict("...")` |
| 45 | `InvalidOperation("This todo list is already active.")` | `Conflict("...")` |

#### `TodoListRun.cs` (18 changes)

| Line | Method | Message |
|------|--------|---------|
| 46 | `UpdateResetPolicy()` | "A closed run's policy cannot be updated." |
| 78 | `Close()` | "The run is already closed." |
| 100 | `ResetInternal()` | "A closed run cannot be reset." |
| 105 | `ResetInternal()` | "The run cannot be reset before the scheduled time." |
| 144 | `AddTodoItem()` | "Items couldn't be added to a closed run." |
| 156 | `DeleteItem()` | "Items couldn't be deleted from a closed run." |
| 169 | `MakeShared()` | "A closed run couldn't get modified." |
| 171 | `MakeShared()` | "This run is already shared." |
| 181 | `MakePrivate()` | "A closed run couldn't get modified." |
| 183 | `MakePrivate()` | "This run is already private." |
| 198 | `MarkItemComplete()` | "A closed run couldn't get modified." |
| 211 | `MarkItemIncomplete()` | "A closed run couldn't get modified." |
| 224 | `UpdateNotes()` | "A closed run couldn't get modified." |
| 239 | `AssignItemToMember()` | "A closed run couldn't get modified." |
| 256 | `ChangeItemDescription()` | "A closed run couldn't get modified." |
| 269 | `AddMember()` | "A closed run couldn't get modified." |
| 273 | `AddMember()` | "Couldn't add members to a private group." |
| 284 | `RemoveMember()` | "A closed run couldn't get modified." |

All change from `DomainError.InvalidOperation("...")` to `DomainError.Conflict("...")`.

### Step 5 — Keep these 3 as `InvalidOperation` (400)

These are genuine bad-request scenarios (invalid input or business invariant violations):

| File | Method | Message | Rationale |
|------|--------|---------|-----------|
| `TodoListRun.cs` | `AssignItemToMember()` line 241 | "not a member of the run" | Caller supplied a member ID that isn't a member |
| `TodoListRun.cs` | `RemoveMember()` line 288 | "Owner couldn't be removed" | Business invariant — can't remove owner regardless of state |
| `TodoListRun.cs` | `RemoveMember()` line 290 | "not a member of this run" | Caller supplied a user ID that isn't a member |

### Step 6 — Update `[ProducesResponseType]` on all controllers

Add `[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]` to every action that can now return a conflict. Some already declare it for `DuplicateEntities` — they can keep the single attribute.

| Controller | Action | 409 already declared? | Action |
|-----------|--------|----------------------|--------|
| `TemplatesController` | `CreateTodoListTemplateAsync` | Yes | Keep |
| `TemplatesController` | `DeleteTodoListTemplate` | No | **Add** (via service → Archive is dead code though — see note below) |
| `RunsController` | `MakeRunSharedAsync` | No (currently 400) | **Change 400 → 409** |
| `RunsController` | `MakeRunPrivateAsync` | No (currently 400) | **Change 400 → 409** |
| `RunItemsController` | `AddItemToRunAsync` | Yes | Keep |
| `RunItemsController` | `DeleteItemFromRunAsync` | No (currently 400) | **Add 409** (keep 400 for non-conflict errors) |
| `RunItemsController` | `MarkRunItemCompleteAsync` | No (currently 400) | **Add 409** (keep 400) |
| `RunItemsController` | `MarkRunItemIncomplete` | No (currently 400) | **Add 409** (keep 400) |
| `RunItemsController` | `UpdateItemNotesAsync` | No (currently 400) | **Add 409** (keep 400) |
| `RunItemsController` | `ChangeRunItemDescriptionAsync` | No (currently 400) | **Add 409** (keep 400) |
| `RunItemsController` | `AssignRunItemToUserAsync` | No (currently 400) | **Add 409** (keep 400) |
| `RunMembersController` | `AddMemberToRunAsync` | Yes | Keep |
| `RunMembersController` | `RemoveMemberFromRunAsync` | No (currently 400) | **Add 409** (keep 400) |

### Step 7 — (Optional) Clean up with Swashbuckle OperationFilter

The `AuthorizedSecurityDocumentFilter` already auto-adds 401 responses. A similar `OperationFilter` could auto-add a 409 `ProblemDetails` response to all `[Authorize]` actions that don't explicitly declare one — but **explicit `[ProducesResponseType]` annotations are preferred** for this project's size (better self-documenting, no magic).

If desired, create `UniTodo/OpenApiFilters/ConflictResponseDocumentFilter.cs`:

```csharp
// Scans all API operations; if the action can return Conflict
// (detected via convention or attribute), ensure 409 is listed.
```

This is optional and not required for the core refactor.

### Step 8 — Update tests

**Domain tests:** Change assertions that check for `DomainErrorCodes.InvalidOperation` to `DomainErrorCodes.Conflict` for the 20 migrated cases.

- `UniTodo.Tests/TodoModuleTests/Domain/TodoListRunTests.cs`
- `UniTodo.Tests/TodoModuleTests/Domain/TodoListTemplateTests.cs`

**Application/Service tests:** Update any mock assertions that expect `InvalidOperation` for the migrated cases.

### Step 9 — Dead code note: `TodoListTemplateService.ArchiveAsync` / `MakeActiveAsync`

These service methods exist but **no controller endpoint calls them**. They call the domain methods that produce the `Conflict` errors. If those endpoints are added later, they will correctly return 409. No action needed now.

---

## Files Changed Summary

```
UniTodo/Modules/Todos/Domain/Common/DomainErrorCodes.cs       # +1 enum value
UniTodo/Modules/Todos/Domain/Common/DomainError.cs             # +1 factory method
UniTodo/Modules/Todos/Api/Extensions/DomainErrorExtensions.cs  # +1 match arm (merge with DuplicateEntities)
UniTodo/Modules/Todos/Domain/Entities/TodoListTemplate.cs      # 2 lines changed
UniTodo/Modules/Todos/Domain/Entities/TodoListRun.cs           # 18 lines changed

# Controllers — add/update [ProducesResponseType(409)]
UniTodo/Modules/Todos/Api/Controllers/TemplatesController.cs
UniTodo/Modules/Todos/Api/Controllers/RunsController.cs
UniTodo/Modules/Todos/Api/Controllers/RunItemsController.cs
UniTodo/Modules/Todos/Api/Controllers/RunMembersController.cs
UniTodo/Modules/Todos/Api/Controllers/TemplateItemsController.cs  # no change needed — already has 409

# Tests
UniTodo.Tests/TodoModuleTests/Domain/TodoListRunTests.cs
UniTodo.Tests/TodoModuleTests/Domain/TodoListTemplateTests.cs
UniTodo.Tests/TodoModuleTests/Application/TodoListTemplateServiceTests.cs    # if applicable
UniTodo.Tests/TodoModuleTests/Application/TodoListRunServiceTests.cs          # if applicable
UniTodo.Tests/TodoModuleTests/Application/TodoListRunItemsServiceTests.cs     # if applicable
UniTodo.Tests/TodoModuleTests/Application/TodoListRunMembersServiceTests.cs   # if applicable
```

---

## Design Decision

| Error Code | HTTP Status | Semantics |
|-----------|-------------|-----------|
| `EntityNotFound` | 404 | Resource doesn't exist |
| `NotAuthorized` | 403 | Actor can't perform action |
| `InvalidOperation` | 400 | Request payload or parameters are invalid |
| `Conflict` | 409 | Request valid but resource state prevents it |
| `DuplicateEntities` | 409 | (Already exists, stays as 409 — now grouped under `ConflictObjectResult`) |

The 400/409 split follows RFC 7231:
- **400 Bad Request** — the request itself is malformed or contains bad data
- **409 Conflict** — the request is well-formed but conflicts with the resource's current state
