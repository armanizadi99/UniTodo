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
        private readonly IRepository<TodoListTemplate> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

public TodoListTemplateService( IRepository<TodoListTemplate> repository, IUnitOfWork unitOfWork, IUserContext userContext  )
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
tl.Id, tl.Name, tl.ResetPolicy, tl.Status, 
tl.CreatedAt, tl.UpdatedAt)).ToList();
        }

public async Task<TodoListTemplateDto> GetTodoListTemplateByIdAsync( int id )
{
        var todoListTemplate = await _repository.GetByIdOrThrowAsync(id);
        return new TodoListTemplateDto(todoListTemplate.Id, todoListTemplate.Name, todoListTemplate.ResetPolicy, todoListTemplate.Status, todoListTemplate.CreatedAt, todoListTemplate.UpdatedAt);
        }
        public async Task<TodoListTemplateDto> CreateTodoListTemplateAsync(CreateTodoListTemplateDto dto)
    {
var todoList = new TodoListTemplate(_userContext.UserId, dto.Name, dto.ResetPolicy!.Value);
        await _repository.AddAsync(todoList);
        await _unitOfWork.SaveChangesAsync();
        return new TodoListTemplateDto(todoList.Id, todoList.Name, todoList.ResetPolicy, todoList.Status, todoList.CreatedAt, todoList.UpdatedAt);
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
