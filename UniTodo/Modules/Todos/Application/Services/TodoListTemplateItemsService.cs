using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoListTemplateItemsService
    {
        private readonly ITodoListTemplateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public TodoListTemplateItemsService(ITodoListTemplateRepository repository, IUnitOfWork unitOfWork, IUserContext userContext)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<TodoItemTemplateDto>> AddTodoItemTemplateAsync(int todoListTemplateId, AddTodoItemTemplateDto dto)
        {
            var todoList = await _repository.GetTodoListTemplateByIdAsync(todoListTemplateId, true);
            if (todoList == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), todoListTemplateId);
            var todoItemTemplate = new TodoItemTemplate(todoListTemplateId, new TodoItemDescription(dto.Description));
            var result = todoList.AddTodoItemTemplate(todoItemTemplate, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<TodoItemTemplateDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return todoItemTemplate.ToDto();
        }

        public async Task<Result> DeleteTodoItemTemplateAsync(int todoListTemplateId, int todoItemTemplateId)
        {
            var todoItemTemplateToDeleteParent = await _repository.GetTodoListTemplateByIdAsync(todoListTemplateId, true);
            if (todoItemTemplateToDeleteParent == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), todoListTemplateId);
            var result = todoItemTemplateToDeleteParent.Delete(todoItemTemplateId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<IReadOnlyList<TodoItemTemplateDto>>> GetTodoItemTemplatesAsync(int todoListTemplateId)
        {
            var todoList = await _repository.GetTodoListTemplateByIdAsync(todoListTemplateId, true);
            if (todoList == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), todoListTemplateId);
            if (todoList.OwnerId != _userContext.UserId)
                return DomainError.NotAuthorized();
            return todoList.TodoItemTemplates.Select(t => t.ToDto()).ToList();
        }
    }
}
