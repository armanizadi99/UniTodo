using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing todo list templates.
    /// </summary>
    [ApiController]
    [Route("api/templates")]
    [Authorize]
    public class TemplateController : ControllerBase
    {
        private readonly TodoListTemplateService _TodoListTemplateService;

        public TemplateController(TodoListTemplateService todoListTemplateService)
        {
            _TodoListTemplateService = todoListTemplateService;
        }

        /// <summary>
        /// Retrieves all todo list templates belonging to the current authenticated user.
        /// </summary>
        /// <returns>A list of todo list templates for the current user.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTodoListTemplatesForCurrentUserAsync()
        {
            var result = await _TodoListTemplateService.GetUserTodoListsAsync();
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new todo list template for the current user.
        /// </summary>
        /// <param name="dto">The data transfer object containing template details.</param>
        /// <returns>The created todo list template.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto)
        {
            var result = await _TodoListTemplateService.CreateTodoListTemplateAsync(dto);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return CreatedAtRoute("GetTodoListTemplateById", new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Retrieves a specific todo list template by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the todo list template.</param>
        /// <returns>The requested todo list template.</returns>
        [HttpGet("{id:int:min(1)}", Name = "GetTodoListTemplateById")]
        public async Task<IActionResult> GetTodoListTemplateByIdAsync([FromRoute] int id)
        {
            var result = await _TodoListTemplateService.GetTodoListTemplateByIdAsync(id);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a todo list template.
        /// </summary>
        /// <param name="id">The identifier of the todo list template to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> DeleteTodoListTemplate([FromRoute] int id)
        {
            var result = await _TodoListTemplateService.DeleteTodoListAsync(id);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Adds a new todo item template to a specific todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the parent todo list template.</param>
        /// <param name="dto">The data transfer object containing todo item template details.</param>
        /// <returns>The created todo item template.</returns>
        [HttpPost("{todoListTemplateId:int:min(1)}/item-templates/")]
        public async Task<IActionResult> AddTodoItemTemplate([FromRoute] int todoListTemplateId, [FromBody] AddTodoItemTemplateDto dto)
        {
            var result = await _TodoListTemplateService.AddTodoItemTemplateAsync(todoListTemplateId, dto);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a specific todo item template from a todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the parent todo list template.</param>
        /// <param name="todoItemTemplateId">The identifier of the todo item template to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{todoListTemplateId:int:min(1)}/template-items/{todoItemTemplateId:int}")]
        public async Task<IActionResult> DeleteTodoItemTemplateAsync([FromRoute] int todoListTemplateId, [FromRoute] int todoItemTemplateId)
        {
            var result = await _TodoListTemplateService.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Retrieves all todo item templates associated with a specific todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the todo list template.</param>
        /// <returns>A list of todo item templates for the specified todo list template.</returns>
        [HttpGet("{todoListTemplateId:int:min(1)}/item-templates")]
        public async Task<IActionResult> GetTodoItemTemplatesAsync([FromRoute] int todoListTemplateId)
        {
            var result = await _TodoListTemplateService.GetTodoItemTemplatesAsync(todoListTemplateId);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        private IActionResult MapError(UniTodo.Modules.Todos.Domain.Common.DomainError error)
        {
            return error.Code switch
            {
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.EntityNotFound => NotFound(error.Message),
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.NotAuthorized => Forbid(),
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.InvalidOperation => BadRequest(error.Message),
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.DuplicateEntities => Conflict(error.Message),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }
    }
}
