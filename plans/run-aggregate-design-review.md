# Run Aggregate — Design Review & Refactoring Options

## Summary

Why does one aggregate (`Run.cs`, 383 lines) need ~100 test methods (~25% of the whole
test suite)? Because `Run` is a **"god aggregate"**: it owns several distinct
responsibilities (lifecycle, todo items, membership, permissions, scheduling), and its
complexity is concentrated into one class with 14 commands and a 14-branch authorization
switch. The tests are correctly taxing that design — refactoring is *optional* and should
**not** be done purely to reduce the test count. This document explains the diagnosis in
plain terms and lays out 4 concrete refactoring options with tradeoffs, from low-risk to
high-risk.

## Pre-review Verification (baseline)

```bash
dotnet build
dotnet test
# Currently: 428 tests, all passing (~20s including Testcontainer spin-up)
```

---

## Part 1 — Why the aggregate needs 100 tests

### 1.1 One class, 14 commands, multiple responsibilities

`Run.cs` conflates at least three sub-domains:

| Responsibility | Commands |
|----------------|----------|
| Lifecycle (scheduling, sharing, close) | `UpdateResetPolicy`, `Close`, `Reset`, `MakeShared`, `MakePrivate`, `UpdateSettings` |
| Todo items | `AddRunItem`, `DeleteItem`, `MarkItemComplete`, `MarkItemIncomplete`, `UpdateNotes`, `ChangeItemDescription`, `AssignItemToMember` |
| Membership & permissions | `AddMember`, `RemoveMember`, `UpdatePermissions` |

Every command gets its own authorization matrix, so the test surface is roughly
`14 ops × (owner | member-with-permission | member-without-permission | non-member | closed state)`.

### 1.2 Structurally identical permission flags → duplicated guard code

`Domain/ValueObjects/RunPermissions.cs` has three booleans that behave identically:

- `MemberAllowedToCompleteUnassignedItems`
- `MemberAllowedToMarkIncompleteUnassignedItems`
- `MemberAllowedToModifyNotesForUnassignedItems`

Each is used by a **copy-pasted guard block** in `Domain/Entities/Run.cs`:

```csharp
// Run.cs:252 (MarkItemComplete), :268 (MarkItemIncomplete), :284 (UpdateNotes)
if (item.AssignedTo == null && actorId != ownerId && !Permissions.X) return DomainError.NotAuthorized();
if (item.AssignedTo != null && item.AssignedTo.Value != actorId)  return DomainError.NotAuthorized();
```

`ChangeItemDescription` (:305) approximates the same rule. This means the same scenario
must be re-verified for each method — the **structural redundancy**, not the count itself.

### 1.3 State machine multiplies every matrix

Every mutation also guards `Status == Closed`,
doubling the (state × actor) combinations. Correct, but again a consequence of one class
owning the whole graph.

### 1.4 Encapsulation leak to callers ("tell, don't ask" violation)

The Application layer reaches **into** the aggregate's collections instead of asking it:

| Call site | Reaches into |
|-----------|--------------|
| `RunService.cs:58`, `:116` | `run.Members.Any(...)` |
| `RunItemsService.cs:53` | `run.CurrentIteration.RunItems` |
| `RunMembersService.cs:28` | `run.Members.Any(...)` |
| `RunSettingsService.cs:30`, `RunPermissionsService.cs:29` | `run.Members.Any(...)` |

This leak directly caused the real bug found during integration testing: the repository's
item-scoped query (`RunRepository.cs`, `ThenInclude(i => i.RunItems.Where(item => item.Id == itemId))`)
silently materialized only the target item, so the domain's duplicate-description check in
`ChangeItemDescription` couldn't see sibling items and returned 204 instead of the intended 409.
We fixed it in `RunItemsService.ChangeRunItemDescriptionAsync` by loading the full iteration.

### 1.5 What the design gets RIGHT

- Tight transaction boundary: items/members/settings change together, always consistent.
  For a collaborative-todo app, "one aggregate owns the whole run graph" is legitimate.
- Logic is deterministic and well-guarded — which is exactly why it *could* be tested 100 ways.

**Bottom line:** the 100 tests aren't wasteful. They're the honest price of concentrated
complexity. Let the count be the *symptom*, not the target.

---

## Part 2 — Refactoring options (ranked by risk)

### Option A (LOW RISK) — Extract the item-authorization guard

**Goal:** De-duplicate the 3–4 copy-pasted "can this actor touch this item?" blocks.

**Changes:**
```csharp
// Run.cs — one method instead of 3 inlined copies
private bool CanModifyItem(RunItem item, UserId actorId, bool allowedForUnassigned) =>
    (item.AssignedTo != null && item.AssignedTo.Value != actorId) ||
    (item.AssignedTo == null && actorId != ownerId && !allowedForUnassigned);
```
- Replace blocks at `Run.cs:252`, `:268`, `:284` (and align `:305`).
- **Impact:** Existing code only; no API/DB changes. `MarkItemComplete`/`MarkItemIncomplete`/
  `UpdateNotes` bodies shrink.

**Test effect:** The auth combinations can be exercised once per guard, consolidating the
~3-4 near-duplicate `WhenMemberWithoutPermission`/`WhenNotAssignee` pairs per method into
one shared set. Modest count reduction, real maintenance gain.

**Maintenance cost:** Low.

---

### Option B (MEDIUM RISK) — Consolidate the three "unassigned-items" permission flags

**Goal:** Collapse the structurally identical "member may act on unassigned items" flags.

**Changes:**
- `Domain/ValueObjects/RunPermissions.cs` + `RunConfiguration`/DB columns:
  replace `MemberAllowedToCompleteUnassignedItems`, `MemberAllowedToMarkIncompleteUnassignedItems`,
  `MemberAllowedToModifyNotesForUnassignedItems` with a single concept (e.g.
  `MemberAllowedToManageUnassignedItems`) — or two (`complete` + `edit`).
- `UpdateRunPermissionsDto` + `RunPermissionsService` + API docs/OpenAPI.
- Requires an EF migration + adjusting the `RunPermissionsServiceTests`/`RunPermissionsTests`.

**Test effect:** Directly halves a whole class of matrix combinations.
**Risk:** API/DB contract change — do this **only if** the product actually wants this
coarser permission model. If per-action granularity is a product requirement, **skip**.

**Maintenance cost:** Medium.

---

### Option C (HIGH RISK) — Split responsibility off the aggregate

**Goal:** Move the item commands onto the iteration / items sub-domain, and/or extract the
scheduling math.

**Changes:**
- **C1 — Items:** move `AddRunItem`..`ChangeItemDescription` onto `RunIteration` (or a
  `RunItem` aggregate). Services currently need `run.CurrentIteration.RunItems` anyway,
  so the aggregate boundary is already soft.
- **C2 — Scheduling:** extract `UpdateResetsAt`/`CalculateNextWeeklyReset`
  (`Run.cs:83-111`) into a dedicated `ResetScheduleCalculator` class. Isolates the ~10
  timezone/offset tests.

**Impact:** Touches services, repository, controllers, possibly DB. Large blast radius,
highest churn.
**Test effect:** Shrinks `RunTests` most sharply, but the same behavior just relocates to
other test classes — total suite size barely changes.

**Maintenance cost:** High. **Recommend deferring** unless the domain is genuinely going to
grow (new item kinds, per-item rules, etc.).

---

### Option D (LOW RISK) — Reduce the cross-layer re-assertion

**Goal:** Trim ~6 near-duplicate `WhenNotMember...NotAuthorized` / `WhenNotOwner...NotAuthorized`
pairs in `RunTests.cs` into `[Theory]` data-driven tests.

**Note:** This is *optional polish*, not a design fix. The three-layer pyramid
(domain → service → integration) intentionally repeats outcomes and should be kept.

**Maintenance cost:** Low.

---

## Part 3 — Recommendation

1. **Do not** refactor purely to reduce test count — the suite runs fast and the design is
   coherent.
2. **If touching this code anyway, pick in this order:**
   - Option A (guard extraction) — clean win, low risk.
   - Option B — only if the product accepts a coarser permission model.
   - Option C — only if the domain is about to grow.
3. The authorization encasulation leak (`run.Members`/`CurrentIteration` reached into by
   services) is the most likely source of future bugs — the domain-owner methods
   (`CanBeAccessedBy`, `CanBeDeletedBy`, `IsOwnedBy`) proposed in
   `plans/fix-misplaced-domain-logic.md` directly address this and are a good first move.

## Part 4 — Verification (after any option)

```bash
dotnet build
dotnet test
# Domain/Service tests should stay green; adjust any unit tests that referenced the
# inlined guard behavior (Option A) or changed DTO shape (Option B).
```
