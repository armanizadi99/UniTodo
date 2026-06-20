using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
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
        public async Task<IActionResult> GetCurrentUserActiveRunsAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserActiveTodoRunsAsync(cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new private empty todo list run.
        /// </summary>
        /// <param name="dto">The data transfer object containing run creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo list run.</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePrivateEmptyRunAsync([FromBody] CreateTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new todo list run from a template.
        /// </summary>
        /// <param name="templateId">The identifier of the template to create the run from.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo list run.</returns>
        [HttpPost("from-template/{templateId:int:min(1)}")]
        public async Task<IActionResult> CreateRunFromTemplateAsync([FromRoute] int templateId, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return CreatedAtRoute("GetRunById", new { runId = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Retrieves a specific todo list run by its identifier.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested todo list run.</returns>
        [HttpGet("{runId:int:min(1)}", Name = "GetRunById")]
        public async Task<IActionResult> GetRunByIdAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            return Ok();
        }

        /// <summary>
        /// Makes a todo list run shared, allowing other members to join.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/make-shared")]
        public async Task<IActionResult> MakeRunSharedAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeTodoListRunSharedAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Makes a todo list run private, removing shared access.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/make-private")]
        public async Task<IActionResult> MakeRunPrivateAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeTodoListRunPrivateAsync(runId, cancellationToken);
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
