namespace UniTodo.Modules.Todos.Domain.Common
{
    public record struct  DomainError(DomainErrorCodes Code, string Message = "");
}
