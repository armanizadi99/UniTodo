using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing runs (active instances of templates).
    /// </summary>
    [ApiController]
    [Route("api/runs")]
    [Authorize]
    public class RunsController : ControllerBase
    {
        private readonly RunService _service;

        public RunsController(RunService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all active runs for the current authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active runs for the current user.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RunDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUserActiveRunsAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserActiveRunsAsync(cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new private empty run.
        /// </summary>
        /// <param name="dto">The data transfer object containing run creation details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created run.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RunDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePrivateEmptyRunAsync([FromBody] CreateRunDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateRunAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new run from a template.
        /// </summary>
        /// <param name="templateId">The identifier of the template to create the run from.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created run.</returns>
        [HttpPost("from-template/{templateId:int:min(1)}")]
        [ProducesResponseType(typeof(RunDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateRunFromTemplateAsync([FromRoute] int templateId, CancellationToken cancellationToken)
        {
            var result = await _service.CreateRunFromTemplateAsync(templateId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return CreatedAtRoute("GetRunById", new { runId = result.Value.Id }, result.Value);
        }

        /// <summary>
        /// Retrieves a specific run by its identifier.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The requested run.</returns>
        [HttpGet("{runId:int:min(1)}", Name = "GetRunById")]
        [ProducesResponseType(typeof(RunDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRunByIdAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetRunByIdAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Makes a run shared, allowing other members to join.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/make-shared")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MakeRunSharedAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeRunSharedAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Makes a run private, removing shared access.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/make-private")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MakeRunPrivateAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeRunPrivateAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Closes a run, preventing further modifications.
        /// </summary>
        /// <param name="runId">The identifier of the run to close.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/close")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CloseRunAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.CloseRunAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Resets a run, creating a new iteration with copies of the current incomplete items.
        /// </summary>
        /// <param name="runId">The identifier of the run to reset.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ResetRunAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.ResetRunAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Retrieves the history of a run (all closed iterations with their items).
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of closed iterations with their items.</returns>
        [HttpGet("{runId:int:min(1)}/history")]
        [ProducesResponseType(typeof(IReadOnlyList<RunIterationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRunHistoryAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetRunHistoryAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates the reset policy of a run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The new reset policy settings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{runId:int:min(1)}/reset-policy")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateRunResetPolicyAsync([FromRoute] int runId, [FromBody] UpdateResetPolicyDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateRunResetPolicyAsync(runId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }
    }
}
