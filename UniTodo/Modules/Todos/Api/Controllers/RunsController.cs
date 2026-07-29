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
        /// <remarks>
        /// Returns all runs where the current user is a member or owner and the run
        /// has not been closed. Active runs are those with a status of Active or Paused.
        ///
        /// Each run includes its name, status, reset policy, ownership, sharing state,
        /// scheduling settings, and member permissions.
        ///
        /// Results are scoped to the current authenticated user.
        /// </remarks>
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
        /// Retrieves all closed runs for the current authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of closed runs for the current user.</returns>
        /// <remarks>
        /// Returns all runs where the current user is a member or owner and the run
        /// has been closed. Closed runs are read-only and cannot be modified.
        ///
        /// Results include the same run details as the active runs endpoint.
        /// Results are scoped to the current authenticated user.
        /// </remarks>
        [HttpGet("closed")]
        [ProducesResponseType(typeof(IReadOnlyList<RunDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUserClosedRunsAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserClosedRunsAsync(cancellationToken);
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
        /// <remarks>
        /// Creates a new run owned by the current authenticated user.
        /// The run is created as private (not shared) and starts with no items.
        ///
        /// Use the run items endpoints to populate the run after creation, or use
        /// the create-from-template endpoint to create a pre-populated run.
        ///
        /// The returned run includes its initial state, default permissions,
        /// and default scheduling settings.
        /// </remarks>
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
        /// <remarks>
        /// Creates a new run by copying all item templates from the specified template
        /// into the run's first iteration. The template must belong to the current
        /// authenticated user.
        ///
        /// The created run inherits the template's name. Unlike creating an empty run,
        /// this endpoint immediately populates the run with items ready to be completed.
        ///
        /// Returns 201 Created with a Location header on success.
        /// Returns 403 Forbidden if the template belongs to a different user.
        /// Returns 404 Not Found if the template does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Returns a single run by its integer ID.
        /// The current user must be a member or owner of the run.
        ///
        /// The response includes the run's full state: name, status, reset policy,
        /// ownership, sharing state, scheduling settings, and member permissions.
        ///
        /// Returns 403 Forbidden if the current user is not a member or owner.
        /// Returns 404 Not Found if no run with the given ID exists.
        /// </remarks>
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
        /// <remarks>
        /// Changes the run's visibility from private to shared. Once shared, the
        /// owner can add members using the run members endpoints.
        /// Only the owner of the run can perform this action.
        ///
        /// When a run becomes shared, member permissions control what non-owner
        /// members are allowed to do. Default permissions allow most actions.
        ///
        /// Returns 400 Bad Request if the run is already shared.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Changes the run's visibility from shared back to private.
        /// All existing non-owner members are removed from the run.
        /// Only the owner of the run can perform this action.
        ///
        /// After this operation, only the owner can access and modify the run.
        ///
        /// Returns 400 Bad Request if the run is already private.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Closes the run, making it read-only. After closing, no modifications
        /// to items, members, permissions, or settings are allowed.
        /// Only the owner of the run can close it.
        ///
        /// The run remains accessible for reading (items, history, members) and
        /// appears in the closed runs list. Closed runs can be deleted permanently.
        ///
        /// Returns 400 Bad Request if the run has already been closed.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Resets the run by archiving the current iteration and creating a new
        /// active iteration. Only incomplete items are carried over to the new
        /// iteration — completed items stay in the archived iteration.
        ///
        /// The reset behavior is governed by the run's reset policy:
        /// - Manual: The run must be reset explicitly via this endpoint.
        /// - Daily: The run automatically resets each day.
        /// - Weekly: The run automatically resets each week.
        ///
        /// Completed items can be reviewed in the run history.
        /// Only the owner of the run can reset it.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Returns all archived (closed) iterations of the run, each containing
        /// the items as they existed when that iteration was closed.
        ///
        /// Iterations are created when a run is reset. The current active iteration
        /// is not included — only past, archived iterations are returned.
        ///
        /// Use this endpoint to review historical completion data and track
        /// what was done in previous resets.
        ///
        /// Returns 403 Forbidden if the current user is not a member or owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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
        /// Deletes a run permanently. Only the owner can delete a run.
        /// </summary>
        /// <param name="runId">The identifier of the run to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        /// <remarks>
        /// Permanently deletes the run and all associated data: iterations, items,
        /// members, permissions, and settings. This action is irreversible.
        ///
        /// Both active and closed runs can be deleted. Only the owner of the run
        /// can perform this action.
        ///
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpDelete("{runId:int:min(1)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveRunAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.RemoveRunAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Updates the reset policy of a run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The new reset policy settings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        /// <remarks>
        /// Changes the reset policy that governs how and when the run's items reset.
        /// Supported policies:
        /// - Manual: Resets only when explicitly requested via the reset endpoint.
        /// - Daily: The run resets automatically each day (incomplete items carry over).
        /// - Weekly: The run resets automatically each week (incomplete items carry over).
        ///
        /// Only the owner of the run can update the reset policy.
        ///
        /// Returns 400 Bad Request if the run is closed or the policy value is invalid.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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