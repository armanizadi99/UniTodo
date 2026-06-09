namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoListRunMemberDto(int Id, Guid UserId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
