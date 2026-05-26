namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoItemTemplateDto(int Id, string Description, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
