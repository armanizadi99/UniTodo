using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing todo item templates within a todo list template.
    /// </summary>
    [ApiController]
    [Route("api/templates/{todoListTemplateId:int:min(1)}/items")]
    [Authorize]
    public class TemplateItemsController : ControllerBase
    {
        private readonly TodoListTemplateItemsService _service;

        public TemplateItemsController(TodoListTemplateItemsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Adds a new todo item template to a specific todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the parent todo list template.</param>
        /// <param name="dto">The data transfer object containing todo item template details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo item template.</returns>
        /// <remarks>
        /// Adds a new item to an existing template. The template must belong to the
        /// current authenticated user.
        ///
        /// Item templates define what items will be copied when a run is created
        /// from this template. They serve as the initial to-do items for new runs.
        ///
        /// Returns 403 Forbidden if the template belongs to a different user.
        /// Returns 404 Not Found if the template does not exist.
        /// Returns 409 Conflict if an item template with the same description
        /// already exists in this template.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(TodoItemTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddTodoItemTemplate([FromRoute] int todoListTemplateId, [FromBody] AddTodoItemTemplateDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.AddTodoItemTemplateAsync(todoListTemplateId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a specific todo item template from a todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the parent todo list template.</param>
        /// <param name="todoItemTemplateId">The identifier of the todo item template to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        /// <remarks>
        /// Removes a single item template from a template.
        /// The parent template must belong to the current authenticated user.
        ///
        /// Deleting an item template does not affect existing runs that were
        /// already created from this template.
        ///
        /// Returns 403 Forbidden if the template belongs to a different user.
        /// Returns 404 Not Found if the template or item template does not exist.
        /// </remarks>
        [HttpDelete("{todoItemTemplateId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteTodoItemTemplateAsync([FromRoute] int todoListTemplateId, [FromRoute] int todoItemTemplateId, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Retrieves all todo item templates associated with a specific todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the todo list template.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of todo item templates for the specified todo list template.</returns>
        /// <remarks>
        /// Returns all item templates belonging to the specified template.
        /// The template must belong to the current authenticated user.
        /// These are the items that will be copied when creating a run from this template.
        ///
        /// Returns 403 Forbidden if the template belongs to a different user.
        /// Returns 404 Not Found if the template does not exist.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TodoItemTemplateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTodoItemTemplatesAsync([FromRoute] int todoListTemplateId, CancellationToken cancellationToken)
        {
            var result = await _service.GetTodoItemTemplatesAsync(todoListTemplateId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }
    }
}