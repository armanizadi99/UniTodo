# Fix Misplaced Domain Logic

## Summary

Move logic from Application/Infrastructure back into Domain, clean up dead code, fix naming, and make audit fields domain-owned.

## Pre-fix Verification

```bash
dotnet build
dotnet test
```

---

## Step 1 — D1: Remove stray import

**File:** `UniTodo/Modules/Todos/Domain/Entities/TodoListTemplate.cs:1`

Remove `using System.Reflection.Metadata;` — dead/wrong namespace import that references an unrelated NuGet package.

---

## Step 2 — D2: Fully domain-owned audit fields

**Files:** `Domain/Common/EntityBase.cs`, `Infrastructure/Db/TodoDbContext.cs`, all domain entities with mutations

- Add `protected void UpdateTimestamp() => UpdatedAt = DateTimeOffset.UtcNow` to `EntityBase`
- Set `CreatedAt = DateTimeOffset.UtcNow` in `EntityBase` constructor
- Call `UpdateTimestamp()` at the end of **every** domain mutation method:

| Entity | Methods |
|--------|---------|
| `Run` | Constructor, `UpdateSettings()`, `UpdatePermissions()`, `UpdateResetPolicy()`, `Close()`, `ResetInternal()`, `MakeShared()`, `MakePrivate()`, `AddRunItem()`, `DeleteItem()`, `AddMember()`, `RemoveMember()` |
| `TodoListTemplate` | Constructor, `Archive()`, `MakeActive()`, `AddTodoItemTemplate()`, `Delete()` |
| `RunItem` | `MarkComplete()`, `MarkIncomplete()`, `UpdateNotes()`, `ChangeDescription()`, `AssignTo()`, `AssignToNoone()` |
| `RunIteration` | Constructor, `Close()` |

- Remove audit field handling from `TodoDbContext.SaveChangesAsync()`:

```csharp
// REMOVE this entire block:
var entries = ChangeTracker.Entries<IAuditable>();
foreach (var entry in entries) { ... }
```

---

## Step 3 — D3: Remove dead code

**File:** `UniTodo/Modules/Todos/Domain/Common/IdHelper.cs`

Delete the entire file. `GuardAgainstInvalid()` has zero callers across the entire codebase.

---

## Step 4 — D4: Rename `ownerId` → `OwnerId`

**Affected files:**

| File | Change |
|------|--------|
| `Domain/Entities/Run.cs` | Rename property + all 26 internal usages |
| `Infrastructure/Db/Configurations/RunConfiguration.cs:14` | `e.ownerId` → `e.OwnerId` |
| `Application/Extensions/RunMappingExtensions.cs:14` | `run.ownerId.Value` → `run.OwnerId.Value` |
| `Application/Services/RunService.cs:139` | `run.ownerId` → `run.OwnerId` (then replaced by `CanBeDeletedBy` in Step 5) |

After rename, generate migration:

```bash
dotnet ef migrations add RenameRunOwnerId --project UniTodo --context TodoDbContext
```

---

## Step 5 — A1/A2/A3: Add auth methods to domain entities

### Add to `Domain/Entities/Run.cs`:

```csharp
public bool CanBeAccessedBy(UserId userId) =>
    _members.Any(m => m.UserId == userId);

public bool CanBeDeletedBy(UserId userId) =>
    ownerId == userId;
```

### Add to `Domain/Entities/TodoListTemplate.cs`:

```csharp
public bool IsOwnedBy(UserId userId) =>
    OwnerId == userId;
```

### Replace in Application layer (9 call sites):

| File | Line | Old Code | New Code |
|------|------|----------|----------|
| `RunService.cs` | 58 | `!run.Members.Any(m => m.UserId == _userContext.UserId)` | `!run.CanBeAccessedBy(_userContext.UserId)` |
| `RunService.cs` | 116 | `!run.Members.Any(m => m.UserId == _userContext.UserId)` | `!run.CanBeAccessedBy(_userContext.UserId)` |
| `RunService.cs` | 139 | `run.ownerId != _userContext.UserId` | `!run.CanBeDeletedBy(_userContext.UserId)` |
| `RunItemsService.cs` | 53 | `!run.Members.Any(...)` | `!run.CanBeAccessedBy(...)` |
| `RunMembersService.cs` | 28 | `!run.Members.Any(...)` | `!run.CanBeAccessedBy(...)` |
| `RunSettingsService.cs` | 30 | `!run.Members.Any(...)` | `!run.CanBeAccessedBy(...)` |
| `RunPermissionsService.cs` | 29 | `!run.Members.Any(...)` | `!run.CanBeAccessedBy(...)` |
| `TodoListTemplateService.cs` | 38 | `todoListTemplate.OwnerId != _userContext.UserId` | `!todoListTemplate.IsOwnedBy(_userContext.UserId)` |
| `TodoListTemplateService.cs` | 59 | `todoListTemplate.OwnerId != _userContext.UserId` | `!todoListTemplate.IsOwnedBy(_userContext.UserId)` |
| `TodoListTemplateItemsService.cs` | 54 | `todoListTemplate.OwnerId != _userContext.UserId` | `!todoListTemplate.IsOwnedBy(_userContext.UserId)` |

---

## Step 6 — I1: Extract "due for reset" predicate

### Add to `Domain/Entities/Run.cs`:

```csharp
public bool IsDueForReset(DateTimeOffset now) =>
    Status == TodoListRunStatus.Active
    && ResetPolicy != ResetPolicy.None
    && ResetsAt <= now;
```

### Update `Infrastructure/Db/Repositories/RunRepository.cs`:

Replace the in-query predicate with the domain method where possible (note: for LINQ-to-SQL, the domain method can be called client-side after filtering by the narrowed `Where` on `Status` and `ResetPolicy`).

---

## Step 7 — A4: Static factory methods for value objects

### Add to `Domain/ValueObjects/RunPermissions.cs`:

```csharp
public static RunPermissions Create(
    bool memberAllowedToCompleteUnassignedItems,
    bool memberAllowedToMarkIncompleteUnassignedItems,
    bool memberAllowedToChangeDescriptions,
    bool memberAllowedToModifyNotesForUnassignedItems,
    bool memberAllowedToAddItems,
    bool memberAllowedToRemoveItems) => new()
    {
        MemberAllowedToCompleteUnassignedItems = memberAllowedToCompleteUnassignedItems,
        MemberAllowedToMarkIncompleteUnassignedItems = memberAllowedToMarkIncompleteUnassignedItems,
        MemberAllowedToChangeDescriptions = memberAllowedToChangeDescriptions,
        MemberAllowedToModifyNotesForUnassignedItems = memberAllowedToModifyNotesForUnassignedItems,
        MemberAllowedToAddItems = memberAllowedToAddItems,
        MemberAllowedToRemoveItems = memberAllowedToRemoveItems
    };
```

### Add to `Domain/ValueObjects/RunSettings.cs`:

```csharp
public static RunSettings Create(
    TimeZoneInfo timeZone,
    DayOfWeek endOfWeekDay,
    bool preserveHistory) => new()
    {
        TimeZone = timeZone,
        EndOfWeekDay = endOfWeekDay,
        PreserveHistory = preserveHistory
    };
```

### Update `Application/Services/RunPermissionsService.cs` (lines ~41-49):

Replace manual construction with `RunPermissions.Create(dto.MemberAllowedToCompleteUnassignedItems!.Value, ...)`.

### Update `Application/Services/RunSettingsService.cs` (lines ~43-48):

Replace manual construction with `RunSettings.Create(timeZone, dto.EndOfWeekDay!.Value, dto.PreserveHistory!.Value)`.

---

## Step 8 — Build, test, verify

```bash
dotnet build
dotnet test
```