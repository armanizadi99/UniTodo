using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Modules.Todos.Application.Services
{
    public interface ITodoListTemplateService
    {
        Task<IReadOnlyList<TodoListTemplateDto>> GetUserTodoListsAsync();
        Task<TodoListTemplateDto> GetTodoListTemplateByIdAsync(int id);
        Task<TodoListTemplateDto> CreateTodoListTemplateAsync(CreateTodoListTemplateDto dto);
        Task DeleteTodoListAsync(int id);
        Task<TodoItemTemplateDto> AddTodoItemTemplateAsync(int todoListTemplateId, AddTodoItemTemplateDto dto);
        Task DeleteTodoItemTemplateAsync(int todoListTemplateId, int todoItemTemplateId);
        Task<IReadOnlyList<TodoItemTemplateDto>> GetTodoItemTemplatesAsync(int todoListTemplateId);
        Task ArchiveAsync(int id);
        Task MakeActiveAsync(int id);
    }
}
