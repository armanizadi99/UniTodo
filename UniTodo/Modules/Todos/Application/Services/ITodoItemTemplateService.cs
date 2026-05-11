using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Modules.Todos.Application.Services
{
    public interface ITodoItemTemplateService
    {
        Task<int> AddTodoItemTemplateAsync( AddTodoItemTemplateDto dto );
        Task DeleteTodoItemTemplateAsync( int id );
    }
}
