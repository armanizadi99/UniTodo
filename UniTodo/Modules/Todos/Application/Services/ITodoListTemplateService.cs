using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Modules.Todos.Application.Services
{
    public interface ITodoListTemplateService
    {
        Task<IReadOnlyList<TodoListTemplateDto>> GetUserTodoListsAsync();
        Task<int> CreateTodoListAsync( CreateTodoListTemplateDto dto );
        Task DeleteTodoListAsync( int id );
        Task ArchiveAsync( int id );
        Task MakeActiveAsync( int id );
    }
}
