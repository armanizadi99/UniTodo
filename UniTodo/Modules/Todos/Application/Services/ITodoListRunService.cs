using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Modules.Todos.Application.Services
{
    public interface ITodoListRunService
    {
        Task<TodoListRunDto> CreateTodoListRunFromTemplateAsync(int templateId, CancellationToken cancellationToken = default);
        Task<TodoListRunDto> CreateTodoListRunAsync(CancellationToken cancellationToken = default);
        Task DeleteTodoListRunAsync(int id, CancellationToken cancellationToken = default);
        Task MakeTodoListRunSharedAsync(int id, CancellationToken cancellationToken = default);
        Task MakeTodoListRunPrivateAsync(int id, CancellationToken cancellationToken = default);
        Task<TodoItemDto> AddTodoItemToTodoListRunAsync(int todoListRunId, AddTodoItemDto dto, CancellationToken cancellationToken = default);
        Task DeleteTodoItemFromTodoListRunAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken = default);
        Task MarkTodoItemCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken = default);
        Task MarkTodoItemNotCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken = default);
        Task UpdateNotesForTodoItemAsync(int todoListRunId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken = default);
        Task ChangeTodoItemDescriptionAsync(int todoListRunId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken = default);
        Task<TodoListRunMemberDto> AddMemberToTodoListRunAsync(int todoListRunId, AddMemberToTodoListRunDto dto, CancellationToken cancellationToken = default);
        Task RemoveMemberFromTodoListRunAsync(int todoListRunId, RemoveMemberFromTodoListRunDto dto, CancellationToken cancellationToken = default);
        Task AsignMemberToItemAsync(int todoListRunId, int todoItemId, AsignMemberToTodoItemDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TodoListRunDto>> GetUserActiveTodoRunsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TodoListRunMemberDto>> GetTodoListRunMembersAsync(int todoListRunId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TodoItemDto>> GetTodoListRunItemsAsync(int todoListRunId, CancellationToken cancellationToken = default);
    }
}
