using Microsoft.EntityFrameworkCore;
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
    internal class TodoListTemplateService : ITodoListTemplateService
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

        async Task<IReadOnlyList<TodoListTemplateDto>> ITodoListTemplateService.GetUserTodoListsAsync()
        {
            var userTodoLists = await _repository.GetUserTodoListTemplatesAsync(_userContext.UserId.Value);
            return userTodoLists
    .Select(tl => new TodoListTemplateDto(
    tl.Id, tl.Name, tl.ResetPolicy, tl.Status,
    tl.CreatedAt, tl.UpdatedAt)).ToList();
        }

        async Task<TodoListTemplateDto> ITodoListTemplateService.GetTodoListTemplateByIdAsync(int id)
        {
            var todoListTemplate = await _repository.GetTodoListTemplateByIdOrThrowAsync(id);
            if (todoListTemplate.OwnerId != _userContext.UserId)
                throw new DomainNotAuthorizedException();
            return new TodoListTemplateDto(todoListTemplate.Id, todoListTemplate.Name, todoListTemplate.ResetPolicy, todoListTemplate.Status, todoListTemplate.CreatedAt, todoListTemplate.UpdatedAt);
        }

        async Task<TodoListTemplateDto> ITodoListTemplateService.CreateTodoListTemplateAsync(CreateTodoListTemplateDto dto)
        {
            if (await _repository.IsNameDuplicateAsync(dto.Name))
                throw new DomainDuplicateEntitiesException("This TodoListTemplate already exists.");

            var todoList = new TodoListTemplate(_userContext.UserId, dto.Name, dto.ResetPolicy!.Value);
            await _repository.AddAsync(todoList);
            await _unitOfWork.SaveChangesAsync();
            return new TodoListTemplateDto(todoList.Id, todoList.Name, todoList.ResetPolicy, todoList.Status, todoList.CreatedAt, todoList.UpdatedAt);
        }

        async Task ITodoListTemplateService.DeleteTodoListAsync(int id)
        {
            var todoListToDelete = await _repository.GetTodoListTemplateByIdOrThrowAsync(id);
            if (todoListToDelete!.OwnerId != _userContext.UserId)
                throw new DomainNotAuthorizedException();
            _repository.Remove(todoListToDelete);
            await _unitOfWork.SaveChangesAsync();
        }

        async Task<TodoItemTemplateDto> ITodoListTemplateService.AddTodoItemTemplateAsync(int todoListTemplateId, AddTodoItemTemplateDto dto)
        {
            var todoList = await _repository.GetTodoListTemplateByIdOrThrowAsync(todoListTemplateId, true);
            var todoItemTemplate = new TodoItemTemplate(todoListTemplateId, new TodoItemDescription(dto.Description));
            todoList.AddTodoItemTemplate(todoItemTemplate, _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
            return new TodoItemTemplateDto(todoItemTemplate.Id, todoItemTemplate.Description.Value, todoItemTemplate.CreatedAt, todoItemTemplate.UpdatedAt);
        }

        async Task ITodoListTemplateService.DeleteTodoItemTemplateAsync(int todoListTemplateId, int todoItemTemplateId)
        {
            var todoItemTemplateToDeleteParent = await _repository.GetTodoListTemplateByIdOrThrowAsync(todoListTemplateId, true);
            todoItemTemplateToDeleteParent.Delete(todoItemTemplateId, _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        async Task<IReadOnlyList<TodoItemTemplateDto>> ITodoListTemplateService.GetTodoItemTemplatesAsync(int todoListTemplateId)
        {
            var todoList = await _repository.GetTodoListTemplateByIdOrThrowAsync(todoListTemplateId, true);

            if (todoList.OwnerId != _userContext.UserId)
                throw new DomainNotAuthorizedException();

            return todoList.TodoItemTemplates.Select(t => new TodoItemTemplateDto(t.Id, t.Description.Value, t.CreatedAt, t.UpdatedAt)).ToList();
        }

        async Task ITodoListTemplateService.ArchiveAsync(int id)
        {
            var todoListToArchive = await _repository.GetTodoListTemplateByIdOrThrowAsync(id);
            todoListToArchive.Archive(_userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        async Task ITodoListTemplateService.MakeActiveAsync(int id)
        {
            var todoListToMakeActive = await _repository.GetTodoListTemplateByIdOrThrowAsync(id);
            todoListToMakeActive.MakeActive(_userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
