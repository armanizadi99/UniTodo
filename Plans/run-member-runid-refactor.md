# RunMember → RunId (Guid) Refactoring Plan

Associate `RunMember` with `TodoListRun.RunId` (Guid) instead of `TodoListRun.Id` (int).

## Changes

### 1. `RunMember.cs` — Change FK type
`int RunId` → `Guid RunId`

### 2. `TodoListRunConfiguration.cs` — Retarget FK to `RunId`
Add `.HasPrincipalKey(e => e.RunId)` so the FK targets `TodoListRun.RunId` (Guid).

### 3. `ITodoListRunRepository.cs` — Add Guid lookup method
```csharp
Task<TodoListRun?> GetTodoListRunByRunIdAsync(Guid runId, bool includeItems = false, CancellationToken ct = default);
```
Existing `GetTodoListRunByIdAsync(int id, ...)` stays for other consumers.

### 4. `TodoListRunRepository.cs` — Implement the method
Query by `e.RunId == runId`, include Members (and optionally Items).

### 5. `TodoListRunMembersService.cs` — Switch to Guid
All three methods change from `int todoListRunId` to `Guid todoListRunId` and call `GetTodoListRunByRunIdAsync`.

### 6. `RunMembersController.cs` — Route and params switch to Guid
- Route: `api/runs/{runId:guid}/members`
- `int runId` → `Guid runId` on all actions

### 7. `TodoListRunMemberDto.cs` — Clean up DTO
Remove `Id` (int). Add `RunId` (Guid).
```csharp
public record TodoListRunMemberDto
{
    public Guid RunId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
```

### 8. `RunMemberMappingExtensions.cs` — Update mapping
```csharp
return new TodoListRunMemberDto(
    RunId: member.RunId,
    UserId: member.UserId.Value,
    CreatedAt: member.CreatedAt,
    UpdatedAt: member.UpdatedAt
);
```

### 9. `TodoListRunMembersServiceTests.cs` — Update for Guid
- Service calls use `Guid` (e.g., `run.RunId`)
- Mock `GetTodoListRunByRunIdAsync` instead of `GetTodoListRunByIdAsync`

### 10. New EF migration
```bash
dotnet ef migrations add RenameRunMemberRunIdToGuid --project UniTodo --context TodoDbContext
```
