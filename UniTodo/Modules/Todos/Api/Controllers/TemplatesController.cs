
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class TemplatesController : ControllerBase
    {
        private readonly TodoListTemplateService _service;

        public TemplatesController(TodoListTemplateService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all todo list templates belonging to the current authenticated user.
        /// </summary>
        /// <returns>A list of todo list templates for the current user.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTodoListTemplatesForCurrentUserAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserTodoListsAsync(cancellationToken);
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
        public async Task<IActionResult> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListTemplateAsync(dto, cancellationToken);
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
        public async Task<IActionResult> GetTodoListTemplateByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetTodoListTemplateByIdAsync(id, cancellationToken);
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
        public async Task<IActionResult> DeleteTodoListTemplate([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteTodoListAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
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
