# Add Settings to Runs — Implementation Plan

Add configurable settings to `TodoListRun`: permissions (per-action role requirements), timezone, and first day of the week.

## Design decisions (user-confirmed)

| Decision | Choice |
|----------|--------|
| Roles | `Admin` / `Member` (owner gets full access bypassing role checks) |
| Timezone format | IANA IDs (e.g. `"America/New_York"`) |
| Weekly reset | Resets on the **first** day of the week (not the last) |

---

## Progress

### 1. New enum — `RunRole`

`Domain/Enums/RunRole.cs`

- [ ] Create file with `Admin`, `Member`

---

### 2. New value object — `RunSettings`

`Domain/ValueObjects/RunSettings.cs`

- [ ] `RunSettings` class:
  - `TimeZoneId` (string, default `"UTC"`)
  - `FirstDayOfWeek` (DayOfWeek, default `Monday`)
  - `Permissions` (nested `RunPermissions`)
- [ ] `RunPermissions` class:
  - `AddTasks` (RunRole, default `Admin`)
  - `ModifyItems` (RunRole, default `Admin`)
  - `CompleteTasks` (RunRole, default `Member`)
  - `AssignTasks` (RunRole, default `Admin`)
- [ ] `RunSettings.Default` static property

---

### 3. Update `RunMember`

`Domain/Entities/RunMember.cs`

- [ ] Add `Role` property (default `Member` for non-owner)

---

### 4. Update `TodoListRun` — add Settings + refactor auth

`Domain/Entities/TodoListRun.cs`

- [ ] Add `Settings` property (initialized to `RunSettings.Default`)
- [ ] Add private helper `HasPermission(UserId actorId, RunRole requiredRole)`:
  - Owner → always `true`
  - Member with `Admin` role → allowed for `Admin`-required actions
  - Member with `Member` role → allowed for `Member`-required and below
- [ ] Add private helper `CanCompleteTask(UserId actorId, TodoItem item)`:
  - Owner → `true`
  - `HasPermission(actorId, Settings.Permissions.CompleteTasks)` → `true`
  - Item assigned to actor → `true`
- [ ] Update all auth checks in existing methods:
  - `AddTodoItem`, `DeleteItem`, `MakeShared`, `MakePrivate` → use `HasPermission(actorId, AddTasks / ModifyItems)`
  - `MarkItemComplete`, `MarkItemIncomplete` → use `CanCompleteTask`
  - `UpdateNotes` → use `HasPermission(actorId, ModifyItems)` (currently assigned user can update notes too — preserve that?)
  - `AssignItemToMember`, `ChangeItemDescription` → use `HasPermission(actorId, AssignTasks / ModifyItems)`
  - `AddMember`, `RemoveMember` → owner-only (keep as-is)
  - `UpdateResetPolicy`, `Close`, `Reset` → owner-only (keep as-is)
- [ ] Replace `GetNextSaturday` with `GetNextDayOfWeek(DayOfWeek target)` using `Settings.FirstDayOfWeek`

---

### 5. EF Core configuration

**`TodoListRunConfiguration.cs`**
- [ ] Add JSON column for `Settings` with value converter (serialize/deserialize to JSON string)

**`RunMemberConfiguration.cs`**
- [ ] Add `Role` column with enum-to-int conversion

---

### 6. DTOs

**`TodoListRunDto.cs`**
- [ ] Add `Settings` field

**`TodoListRunMemberDto.cs`**
- [ ] Add `Role` field

**`CreateTodoListRunDto.cs`**
- [ ] Add optional `Settings` property

**`AddMemberToTodoListRunDto.cs`**
- [ ] Add optional `RunRole? Role` property

**NEW: `UpdateRunSettingsDto.cs`**
- [ ] `TimeZoneId` (string?)
- [ ] `FirstDayOfWeek` (DayOfWeek?)
- [ ] `AddTasks` (RunRole?)
- [ ] `ModifyItems` (RunRole?)
- [ ] `CompleteTasks` (RunRole?)
- [ ] `AssignTasks` (RunRole?)

---

### 7. Mapping extensions

**`TodoListRunMappingExtensions.cs`**
- [ ] Map `Settings`

**`RunMemberMappingExtensions.cs`**
- [ ] Map `Role`

---

### 8. Application service

**`TodoListRunService.cs`**
- [ ] Add `UpdateSettingsAsync(int runId, UpdateRunSettingsDto dto, CancellationToken ct)`:
  - Load run
  - Owner-only guard
  - Apply non-null fields from DTO onto `run.Settings`
  - Save

---

### 9. API controller

**`RunsController.cs`**
- [ ] Implement `GET /api/runs/{runId}` (currently a stub returning `Ok()`)
- [ ] Add `PATCH /api/runs/{runId}/settings`

---

### 10. Tests

**Domain: `TodoListRunTests.cs`**
- [ ] `Constructor_WithDefaultSettings_ShouldHaveCorrectDefaults`
- [ ] `HasPermission_Owner_ReturnsTrue`
- [ ] `HasPermission_AdminMember_ReturnsTrueForAdminActions`
- [ ] `HasPermission_Member_ReturnsFalseForAdminActions`
- [ ] `HasPermission_Member_ReturnsTrueForMemberActions`
- [ ] `AddTodoItem_WhenMemberHasPermission_ShouldSucceed`
- [ ] `AddTodoItem_WhenMemberLacksPermission_ShouldReturnNotAuthorized`
- [ ] `MarkItemComplete_WhenMemberButAssignedToMember_ShouldSucceed` (special rule)
- [ ] `WeeklyReset_WithMondayFirstDay_ShouldResetOnMonday`
- [ ] `WeeklyReset_WithSundayFirstDay_ShouldResetOnSunday`

**Domain: `RunSettingsTests.cs`**
- [ ] `Default_ShouldReturnExpectedDefaults`
- [ ] `Serialize_Deserialize_RoundTrip`

**Service: `TodoListRunServiceTests.cs`**
- [ ] `UpdateSettingsAsync_WhenOwner_ShouldUpdateAndSave`

---

### 11. Verification

- [ ] `dotnet build` — compiles clean
- [ ] `dotnet test` — all tests pass (existing + new)
