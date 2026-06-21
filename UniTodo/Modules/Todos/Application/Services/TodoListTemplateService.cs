using System.Threading;
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

        public async Task<Result<IReadOnlyList<TodoListTemplateDto>>> GetUserTodoListsAsync(CancellationToken cancellationToken = default)
        {
            var userTodoLists = await _repository.GetUserTodoListTemplatesAsync(_userContext.UserId.Value, cancellationToken);
            return userTodoLists
                .Select(tl => tl.ToDto()).ToList();
        }

        public async Task<Result<TodoListTemplateDto>> GetTodoListTemplateByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var todoListTemplate = await _repository.GetTodoListTemplateByIdAsync(id, cancellationToken: cancellationToken);
            if (todoListTemplate == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            if (todoListTemplate.OwnerId != _userContext.UserId)
                return DomainError.NotAuthorized();
            return todoListTemplate.ToDto();
        }

        public async Task<Result<TodoListTemplateDto>> CreateTodoListTemplateAsync(CreateTodoListTemplateDto dto, CancellationToken cancellationToken = default)
        {
            if (await _repository.IsNameDuplicateAsync(dto.Name, cancellationToken))
                return DomainError.DuplicateEntities("This TodoListTemplate already exists.");

            var todoList = new TodoListTemplate(_userContext.UserId, dto.Name, dto.ResetPolicy!.Value);
            await _repository.AddAsync(todoList);
            await _unitOfWork.SaveChangesAsync();
            return todoList.ToDto();
        }

        public async Task<Result> DeleteTodoListAsync(int id, CancellationToken cancellationToken = default)
        {
            var todoListToDelete = await _repository.GetTodoListTemplateByIdAsync(id, cancellationToken: cancellationToken);
            if (todoListToDelete == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            if (todoListToDelete!.OwnerId != _userContext.UserId)
                return DomainError.NotAuthorized();
            _repository.Remove(todoListToDelete);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> ArchiveAsync(int id, CancellationToken cancellationToken = default)
        {
            var todoListToArchive = await _repository.GetTodoListTemplateByIdAsync(id, cancellationToken: cancellationToken);
            if (todoListToArchive == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), id);
            var result = todoListToArchive.Archive(_userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MakeActiveAsync(int id, CancellationToken cancellationToken = default)
        {
            var todoListToMakeActive = await _repository.GetTodoListTemplateByIdAsync(id, cancellationToken: cancellationToken);
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