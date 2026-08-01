using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing a run's scheduling settings.
    /// </summary>
    [ApiController]
    [Route("api/runs/{runId:int:min(1)}/settings")]
    [Authorize]
    public class RunSettingsController : TodoControllerBase
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
        public async Task<Result<RunSettingsDto>> GetRunSettingsAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            return await _service.GetRunSettingsAsync(runId, cancellationToken);
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
        /// This is a full replacement — all setting fields are required and must be
        /// included in the request body. The request is rejected with a 400 Bad Request
        /// if any field is missing.
        ///
        /// Settings control when the run is active: time zone, end-of-week day, and
        /// whether historical iterations are preserved.
        ///
        /// Only the owner of the run can update settings.
        ///
        /// Returns 400 Bad Request if the run is closed or any required field is missing.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpPut]
        [ProducesResponseType(typeof(RunSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<Result<RunSettingsDto>> UpdateRunSettingsAsync([FromRoute] int runId, [FromBody] UpdateRunSettingsDto dto, CancellationToken cancellationToken)
        {
            return await _service.UpdateRunSettingsAsync(runId, dto, cancellationToken);
        }
    }
}