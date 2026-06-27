# Introduce `Run` Aggregate

## Problem

- `RunMember.TodoListRunId` FK references `TodoListRun.RunId` (Guid) via `.HasPrincipalKey(e => e.RunId)`
- `RunId` is **not** unique — multiple `TodoListRun` rows share the same `RunId` (iterations after reset)
- The migration `20260626125400_RefactorRunIdToGuid` forced a unique constraint `AK_todoListRuns_RunId`, which:
  - Contradicts `builder.HasIndex(e => e.RunId).IsUnique(false)`
  - Crashed with `SQLite Error 1: 'foreign key mismatch - "ef_temp_runMembers" referencing "todoListRuns"'` because the unique constraint was added after the temp table rebuild
- Domain issue: members belong to the **logical run** (across all iterations), not a specific iteration

## Solution

Introduce a `Run` aggregate — a separate entity representing the logical run that persists across iterations.

### New `Run` entity

| Property | Type | Source |
|----------|------|--------|
| `RunId` | `Guid` (PK) | |
| `Name` | `string` | From `TodoListRun` |
| `ownerId` | `UserId` | From `TodoListRun` |
| `ResetPolicy` | `ResetPolicy` | From `TodoListRun` |
| `IsShared` | `bool` | From `TodoListRun` |
| `Members` | `List<RunMember>` | Collection from `TodoListRun` |

### Modified `TodoListRun` (iteration)

| Removed | Moved to `Run` |
|---------|----------------|
| `Name` | |
| `ownerId` | |
| `ResetPolicy` | |
| `IsShared` | |
| `Members` + member methods | |

| Added | Notes |
|-------|-------|
| `Run` navigation property | FK to `Runs` via `RunId` |

### Modified `RunMember`

- `TodoListRunId` → `RunId` (Guid, FK → `Run.RunId`)
- Navigation property: `TodoListRun` → `Run`
- PK: `(UserId, RunId)` (instead of `(UserId, TodoListRunId)`)

## Affected files

### Domain (2 new, 2 modified)

| File | Change |
|------|--------|
| `Domain/Entities/Run.cs` | **New** — run aggregate entity |
| `Domain/Entities/TodoListRun.cs` | Remove run-level properties/members; add `Run` nav |
| `Domain/Entities/RunMember.cs` | `TodoListRunId` → `RunId`; nav `TodoListRun` → `Run` |

### Infrastructure (4 modified + migration)

| File | Change |
|------|--------|
| `Infrastructure/Db/Configurations/RunConfiguration.cs` | **New** EF config for `Run` |
| `Infrastructure/Db/Configurations/TodoListRunConfiguration.cs` | Update FK to `Run`, remove Members config |
| `Infrastructure/Db/Configurations/RunMemberConfiguration.cs` | PK `(UserId, RunId)`, FK to `Run` |
| `Infrastructure/Db/TodoDbContext.cs` | Add `DbSet<Run>` |
| `Infrastructure/Db/Repositories/TodoListRunRepository.cs` | Update includes/queries |
| Migrations | Scaffold fresh — create `Runs`, populate from distinct `RunId`, drop unique constraint, update FKs |

### Application (3-4 modified)

| File | Change |
|------|--------|
| `Application/Services/TodoListRunMembersService.cs` | Work with `Run.Members` |
| `Application/Services/TodoListRunService.cs` | `MakeShared`/`MakePrivate` on `Run` |
| `Application/Services/TodoListRunItemsService.cs` | Auth check via `Run.Members` |
| `Application/BackgroundServices/ResetPolicyJob.cs` | Reset uses `Run` entity |

### API (no changes expected)

Controllers should remain the same — they already operate on `runId:guid`.

### Tests (5 files)

| File | Change |
|------|--------|
| `TodoListRunTests.cs` | Adapt domain tests to new model |
| `TodoListRunMembersServiceTests.cs` | Adapt service tests |
| `TodoListRunServiceTests.cs` | Adapt service tests |
| `TodoListRunItemsServiceTests.cs` | Adapt service tests |
| `TodoListRunRepositoryTests.cs` | Adapt repository tests |

## Migration strategy

1. **Delete** the failing migration `20260626125400_RefactorRunIdToGuid` (and its `.Designer.cs`)
2. Revert the model snapshot to pre-migration state
3. Implement domain/infra changes above
4. Scaffold a new migration that:
   - Creates `Runs` table (PK = `RunId`)
   - Inserts distinct `RunId` records from existing `todoListRuns` (with `Name`, `ownerId`, `ResetPolicy`, `IsShared`)
   - Drops `AK_todoListRuns_RunId` (unique constraint on `RunId`)
   - Drops `FK_runMembers_todoListRuns_TodoListRunId`
   - Recreates `runMembers` with `RunId` FK to `Runs`
   - Adds `FK_todoListRuns_RunId` to `Runs`
