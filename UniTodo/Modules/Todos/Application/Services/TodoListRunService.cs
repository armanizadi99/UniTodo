using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Application.Services
{
    internal class TodoListRunService
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly ITodoListTemplateRepository _templateRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

public TodoListRunService( ITodoListRunRepository runRepository, ITodoListTemplateRepository templateRepository, IUserContext userContext, IUnitOfWork unitOfWork )
        {
        _runRepository = runRepository;
        _templateRepository = templateRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        }

        Task<TodoListRunDto> CreateTodoListRunFromTemplateAsync(int templateId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TodoListRunDto> CreateTodoListRunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task DeleteTodoListRunAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TodoItemDto> AddTodoItemToTodoListRunAsync(int todoListRunId, AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task DeleteTodoItemFromTodoListRunAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<TodoItemDto>> GetTodoListRunItemsAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<TodoListRunMemberDto>> GetTodoListRunMembersAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<TodoListRunDto>> GetUserActiveTodoRunsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task MakeTodoListRunSharedAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task MakeTodoListRunPrivateAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task MarkTodoItemCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task MarkTodoItemNotCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task UpdateNotesForTodoItemAsync(int todoListRunId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task AsignMemberToItemAsync(int todoListRunId, int todoItemId, AsignMemberToTodoItemDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ChangeTodoItemDescriptionAsync(int todoListRunId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TodoListRunMemberDto> AddMemberToTodoListRunAsync(int todoListRunId, AddMemberToTodoListRunDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task RemoveMemberFromTodoListRunAsync(int todoListRunId, RemoveMemberFromTodoListRunDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}