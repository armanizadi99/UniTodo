using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    internal class TodoItemTemplateService : ITodoItemTemplateService
    {
private readonly IRepository<TodoItemTemplate> _todoItemTemplateRepository;
        private readonly IRepository<TodoListTemplate> _todoListRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

public TodoItemTemplateService( IRepository<TodoItemTemplate> todoItemTemplateRepository, IRepository<TodoListTemplate> todoListRepository, IUserContext userContext, IUnitOfWork unitOfWork )
        {
        _todoItemTemplateRepository = todoItemTemplateRepository;
        _todoListRepository = todoListRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        }

        async Task<int> ITodoItemTemplateService.AddTodoItemTemplateAsync( AddTodoItemTemplateDto dto)
{
        var todoList = await _todoListRepository.GetByIdOrThrowAsync(dto.TodoListId!.Value);
        if (todoList.OwnerId != _userContext.UserId)
            throw new DomainNotAuthorizedException();
        var todoItemTemplate = new TodoItemTemplate(dto.TodoListId!.Value, new TodoItemDescription(dto.Description));
        await _todoItemTemplateRepository.AddAsync(todoItemTemplate);
        await _unitOfWork.SaveChangesAsync();
        return todoItemTemplate.Id;
        }

async Task ITodoItemTemplateService.DeleteTodoItemTemplateAsync( int id)
{
var todoItemTemplateToDelete = await _todoItemTemplateRepository.GetByIdOrThrowAsync(id, i => i.TodoList);
if(todoItemTemplateToDelete.TodoList.OwnerId != _userContext.UserId)
throw new DomainNotAuthorizedException();
        _todoItemTemplateRepository.Remove(todoItemTemplateToDelete);
        await _unitOfWork.SaveChangesAsync();
        }
    }
}
