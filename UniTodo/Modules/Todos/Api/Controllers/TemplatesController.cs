using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
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
        [ProducesResponseType(typeof(IReadOnlyList<TodoListTemplateDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTodoListTemplatesForCurrentUserAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserTodoListsAsync(cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new todo list template for the current user.
        /// </summary>
        /// <param name="dto">The data transfer object containing template details.</param>
        /// <returns>The created todo list template.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TodoListTemplateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListTemplateAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return CreatedAtRoute("GetTodoListTemplateById", new { id = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Retrieves a specific todo list template by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the todo list template.</param>
        /// <returns>The requested todo list template.</returns>
        [HttpGet("{id:int:min(1)}", Name = "GetTodoListTemplateById")]
        [ProducesResponseType(typeof(TodoListTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTodoListTemplateByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetTodoListTemplateByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a todo list template.
        /// </summary>
        /// <param name="id">The identifier of the todo list template to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{id:int:min(1)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTodoListTemplate([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteTodoListAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }
    }
}
