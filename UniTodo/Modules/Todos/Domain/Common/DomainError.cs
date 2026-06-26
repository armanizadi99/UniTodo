namespace UniTodo.Modules.Todos.Domain.Common
{
    public record struct DomainError(DomainErrorCodes Code, string Message = "")
    {
        public static DomainError EntityNotFound(string entityName, int entityId)
        {
            return new DomainError(DomainErrorCodes.EntityNotFound, $"'{entityName}' with id {entityId}' is not found.");
        }

        public static DomainError EntityNotFound(string entityName, Guid entityId)
        {
            return new DomainError(DomainErrorCodes.EntityNotFound, $"'{entityName}' with id {entityId}' is not found.");
        }

        public static DomainError NotAuthorized(string message = "")
        {
            return new DomainError(DomainErrorCodes.NotAuthorized, message);
        }

        public static DomainError InvalidOperation(string message)
        {
            return new DomainError(DomainErrorCodes.InvalidOperation, message);
        }

        public static DomainError DuplicateEntities(string message)
        {
            return new DomainError(DomainErrorCodes.DuplicateEntities, message);
        }
    }
}
