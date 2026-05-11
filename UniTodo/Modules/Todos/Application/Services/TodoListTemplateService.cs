using UniTodo.Modules.Todos.Application.Common;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoListTemplateService : ITodoListTemplateService
    {
        private readonly IRepository<TodoListTemplate, TodoListTemplateId> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

public TodoListTemplateService( IRepository<TodoListTemplate, TodoListTemplateId> repository, IUnitOfWork unitOfWork, IUserContext userContext  )
        {
        _repository = repository;
_unitOfWork = unitOfWork;
        _userContext = userContext;
        }

public async Task<IReadOnlyList<TodoListTemplateDto>> GetUserTodoListsAsync()
{
        var userTodoLists = await _repository.GetListAsync(e => e.OwnerId == _userContext.UserId);
        return userTodoLists
.Select(tl => new TodoListTemplateDto(
tl.Id.Value, tl.Name, tl.ResetPolicy, tl.Status, 
tl.CreatedAt, tl.UpdatedAt)).ToList();
        }

public async Task<int> CreateTodoListAsync(CreateTodoListTemplateDto dto)
    {
var todoList = new TodoListTemplate(_userContext.UserId, dto.Name, dto.ResetPolicy!.Value);
        await _repository.AddAsync(todoList);
        await _unitOfWork.SaveChangesAsync();
        return todoList.Id.Value;
        }

public async Task DeleteTodoListAsync(int id)
        {
        var todoListToDelete = await _repository.GetByIdOrThrowAsync(id);
        if (todoListToDelete!.OwnerId != _userContext.UserId)
            throw new DomainNotAuthorizedException();
        _repository.Remove(todoListToDelete);
        await _unitOfWork.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int id)
{
        var todoListToArchive = await _repository.GetByIdOrThrowAsync(id);
        todoListToArchive.Archive(_userContext.UserId);
        }

public async Task MakeActiveAsync(int id)
{
        var todoListToMakeActive = await _repository.GetByIdOrThrowAsync(id);
        todoListToMakeActive.MakeActive(_userContext.UserId);
        }
    }
}
