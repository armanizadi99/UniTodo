using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Application.Services
{
    internal class TodoListRunService : ITodoListRunService
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

        Task<TodoListRunDto> ITodoListRunService.CreateTodoListRunFromTemplateAsync(int templateId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TodoListRunDto> ITodoListRunService.CreateTodoListRunAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.DeleteTodoListRunAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TodoItemDto> ITodoListRunService.AddTodoItemToTodoListRunAsync(int todoListRunId, AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.DeleteTodoItemFromTodoListRunAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<TodoItemDto>> ITodoListRunService.GetTodoListRunItemsAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<TodoListRunMemberDto>> ITodoListRunService.GetTodoListRunMembersAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IReadOnlyList<TodoListRunDto>> ITodoListRunService.GetUserActiveTodoRunsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.MakeTodoListRunSharedAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.MakeTodoListRunPrivateAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.MarkTodoItemCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.MarkTodoItemNotCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.UpdateNotesForTodoItemAsync(int todoListRunId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.AsignMemberToItemAsync(int todoListRunId, int todoItemId, AsignMemberToTodoItemDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.ChangeTodoItemDescriptionAsync(int todoListRunId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<TodoListRunMemberDto> ITodoListRunService.AddMemberToTodoListRunAsync(int todoListRunId, AddMemberToTodoListRunDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ITodoListRunService.RemoveMemberFromTodoListRunAsync(int todoListRunId, RemoveMemberFromTodoListRunDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}