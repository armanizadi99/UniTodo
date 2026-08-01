using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Attributes;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing todo list templates.
    /// </summary>
    [ApiController]
    [Route("api/templates")]
    [Authorize]
    public class TemplatesController : TodoControllerBase
    {
        private readonly TodoListTemplateService _service;

        public TemplatesController(TodoListTemplateService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all todo list templates belonging to the current authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of todo list templates for the current user.</returns>
        /// <remarks>
        /// Returns all templates owned by the current authenticated user.
        /// Templates are reusable blueprints that can be used to create runs.
        /// Each template contains a name and a set of item templates that define
        /// the default items copied into a run when it is created from the template.
        ///
        /// Results are scoped to the authenticated user — users cannot see templates
        /// owned by other users through this endpoint.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TodoListTemplateDto>), StatusCodes.Status200OK)]
        public async Task<Result<IReadOnlyList<TodoListTemplateDto>>> GetAllTodoListTemplatesForCurrentUserAsync(CancellationToken cancellationToken)
        {
            return await _service.GetUserTodoListsAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a new todo list template for the current user.
        /// </summary>
        /// <param name="dto">The data transfer object containing template details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo list template.</returns>
        /// <remarks>
        /// Creates a new template owned by the current authenticated user.
        /// The template name must be unique per user — duplicate names are rejected.
        ///
        /// The returned 201 Created response includes a Location header pointing
        /// to the newly created template. The template starts with no items;
        /// use the template items endpoints to add items after creation.
        ///
        /// Returns 409 Conflict if a template with the same name already exists
        /// for this user.
        /// </remarks>
        [HttpPost]
        [CreatedAtRouteResult("GetTodoListTemplateById")]
        [ProducesResponseType(typeof(TodoListTemplateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<Result<TodoListTemplateDto>> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto, CancellationToken cancellationToken)
        {
            return await _service.CreateTodoListTemplateAsync(dto, cancellationToken);
        }

        /// <summary>
        /// Retrieves a specific todo list template by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the todo list template.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested todo list template.</returns>
        /// <remarks>
        /// Returns a single template by its integer ID.
        /// The template must belong to the current authenticated user.
        ///
        /// Returns 403 Forbidden if the template exists but belongs to a different user.
        /// Returns 404 Not Found if no template with the given ID exists.
        /// </remarks>
        [HttpGet("{id:int:min(1)}", Name = "GetTodoListTemplateById")]
        [ProducesResponseType(typeof(TodoListTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<Result<TodoListTemplateDto>> GetTodoListTemplateByIdAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            return await _service.GetTodoListTemplateByIdAsync(id, cancellationToken);
        }

        /// <summary>
        /// Deletes a todo list template.
        /// </summary>
        /// <param name="id">The identifier of the todo list template to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        /// <remarks>
        /// Permanently deletes a template and all its item templates.
        /// The template must belong to the current authenticated user.
        ///
        /// This operation does not affect runs that were previously created
        /// from this template — those runs are independent copies.
        /// This action is irreversible.
        ///
        /// Returns 403 Forbidden if the template belongs to another user.
        /// Returns 404 Not Found if no template with the given ID exists.
        /// </remarks>
        [HttpDelete("{id:int:min(1)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<Result> DeleteTodoListTemplate([FromRoute] int id, CancellationToken cancellationToken)
        {
            return await _service.DeleteTodoListAsync(id, cancellationToken);
        }
    }
}