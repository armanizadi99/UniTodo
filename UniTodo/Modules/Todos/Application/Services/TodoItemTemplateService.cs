using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoItemTemplateService : ITodoItemTemplateService
    {
private readonly IRepository<TodoItemTemplate, TodoItemTemplateId> _todoItemTemplateRepository;
        private readonly IRepository<TodoListTemplate, TodoListTemplateId> _todoListRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

public TodoItemTemplateService( IRepository<TodoItemTemplate, TodoItemTemplateId> todoItemTemplateRepository, IRepository<TodoListTemplate, TodoListTemplateId> todoListRepository, IUserContext userContext, IUnitOfWork unitOfWork )
        {
        _todoItemTemplateRepository = todoItemTemplateRepository;
        _todoListRepository = todoListRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        }

        public async Task<int> AddTodoItemTemplateAsync(AddTodoItemTemplateDto dto)
{
        var todoList = await _todoListRepository.GetByIdOrThrowAsync(dto.TodoListId!.Value);
        if (todoList.OwnerId == _userContext.UserId)
            throw new DomainNotAuthorizedException();
        var todoItemTemplate = new TodoItemTemplate(new TodoListTemplateId(dto.TodoListId!.Value), new TodoItemDescription(dto.Description));
        await _todoItemTemplateRepository.AddAsync(todoItemTemplate);
        await _unitOfWork.SaveChangesAsync();
        return todoItemTemplate.Id.Value;
        }

public async Task DeleteTodoItemTemplateAsync(int id)
{
var todoItemTemplateToDelete = await _todoItemTemplateRepository.GetByIdOrThrowAsync(id, i => i.TodoList);
if(todoItemTemplateToDelete.TodoList.OwnerId == _userContext.UserId)
throw new DomainNotAuthorizedException();
        _todoItemTemplateRepository.Remove(todoItemTemplateToDelete);
        await _unitOfWork.SaveChangesAsync();
        }
    }
}
