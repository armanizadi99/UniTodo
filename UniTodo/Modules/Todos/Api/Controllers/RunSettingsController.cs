using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing a run's scheduling settings.
    /// </summary>
    [ApiController]
    [Route("api/runs/{runId:int:min(1)}/settings")]
    [Authorize]
    public class RunSettingsController : ControllerBase
    {
        private readonly RunSettingsService _service;

        public RunSettingsController(RunSettingsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves the scheduling settings for a run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The run's scheduling settings.</returns>
        /// <remarks>
        /// Returns the scheduling settings for the run, which control when the run
        /// is considered active. Settings include start date, end date, and pause
        /// configuration.
        ///
        /// The current user must be a member or owner of the run.
        ///
        /// Returns 403 Forbidden if the current user is not a member or owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(RunSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRunSettingsAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetRunSettingsAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates the scheduling settings for a run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The new settings to apply.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated scheduling settings.</returns>
        /// <remarks>
        /// Completely replaces all scheduling settings for the run with the provided values.
        /// This is a full replacement, not a partial update — any setting not included
        /// in the request will be set to its default value.
        ///
        /// Settings control when the run is active: start date, end date, and whether
        /// the run is currently paused.
        ///
        /// Only the owner of the run can update settings.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpPut]
        [ProducesResponseType(typeof(RunSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateRunSettingsAsync([FromRoute] int runId, [FromBody] UpdateRunSettingsDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateRunSettingsAsync(runId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }
    }
}