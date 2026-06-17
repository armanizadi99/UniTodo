using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoListTemplateService
    {
        private readonly ITodoListTemplateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public TodoListTemplateService(ITodoListTemplateRepository repository, IUnitOfWork unitOfWork, IUserContext userContext)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<IReadOnlyList<TodoListTemplateDto>>> GetUserTodoListsAsync()
        {
            var userTodoLists = await _repository.GetUserTodoListTemplatesAsync(_userContext.UserId.Value);
            return userTodoLists
                .Select(tl => tl.ToDto()).ToList();
        }

        public async Task<Result<TodoListTemplateDto>> GetTodoListTemplateByIdAsync(int id)
        {
            var todoListTemplate = await _repository.GetTodoListTemplateByIdAsync(id);
            if (todoListTemplate == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            if (todoListTemplate.OwnerId != _userContext.UserId)
                return DomainError.NotAuthorized();
            return todoListTemplate.ToDto();
        }

        public async Task<Result<TodoListTemplateDto>> CreateTodoListTemplateAsync(CreateTodoListTemplateDto dto)
        {
            if (await _repository.IsNameDuplicateAsync(dto.Name))
                return DomainError.DuplicateEntities("This TodoListTemplate already exists.");

            var todoList = new TodoListTemplate(_userContext.UserId, dto.Name, dto.ResetPolicy!.Value);
            await _repository.AddAsync(todoList);
            await _unitOfWork.SaveChangesAsync();
            return todoList.ToDto();
        }

        public async Task<Result> DeleteTodoListAsync(int id)
        {
            var todoListToDelete = await _repository.GetTodoListTemplateByIdAsync(id);
            if (todoListToDelete == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            if (todoListToDelete!.OwnerId != _userContext.UserId)
                return DomainError.NotAuthorized();
            _repository.Remove(todoListToDelete);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
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

        public async Task<Result> ArchiveAsync(int id)
        {
            var todoListToArchive = await _repository.GetTodoListTemplateByIdAsync(id);
            if (todoListToArchive == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            var result = todoListToArchive.Archive(_userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MakeActiveAsync(int id)
        {
            var todoListToMakeActive = await _repository.GetTodoListTemplateByIdAsync(id);
            if (todoListToMakeActive == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            var result = todoListToMakeActive.MakeActive(_userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}