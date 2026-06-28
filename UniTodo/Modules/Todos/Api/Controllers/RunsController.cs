using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing todo list runs (active instances of templates).
    /// </summary>
    [ApiController]
    [Route("api/runs")]
    [Authorize]
    public class RunsController : ControllerBase
    {
        private readonly TodoListRunService _service;

        public RunsController(TodoListRunService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all active todo list runs for the current authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active todo list runs for the current user.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TodoListRunDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUserActiveRunsAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserActiveTodoRunsAsync(cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new private empty todo list run.
        /// </summary>
        /// <param name="dto">The data transfer object containing run creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo list run.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TodoListRunDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePrivateEmptyRunAsync([FromBody] CreateTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new todo list run from a template.
        /// </summary>
        /// <param name="templateId">The identifier of the template to create the run from.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo list run.</returns>
        [HttpPost("from-template/{templateId:int:min(1)}")]
        [ProducesResponseType(typeof(TodoListRunDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRunFromTemplateAsync([FromRoute] int templateId, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return CreatedAtRoute("GetRunByRunId", new { runId = result.Value.RunId }, result.Value);
        }

        /// <summary>
        /// Retrieves a specific todo list run by its run identifier.
        /// </summary>
        /// <param name="runId">The run identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested todo list run.</returns>
        [HttpGet("{runId:guid}", Name = "GetRunByRunId")]
        [ProducesResponseType(typeof(TodoListRunDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRunByRunIdAsync([FromRoute] Guid runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetTodoListRunByRunIdAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Makes a todo list run shared, allowing other members to join.
        /// </summary>
        /// <param name="runId">The run identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:guid}/make-shared")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MakeRunSharedAsync([FromRoute] Guid runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeTodoListRunSharedAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Makes a todo list run private, removing shared access.
        /// </summary>
        /// <param name="runId">The run identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:guid}/make-private")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MakeRunPrivateAsync([FromRoute] Guid runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeTodoListRunPrivateAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }
    }
}
