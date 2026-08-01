using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing a run's member permissions.
    /// </summary>
    [ApiController]
    [Route("api/runs/{runId:int:min(1)}/permissions")]
    [Authorize]
    public class RunPermissionsController : TodoControllerBase
    {
        private readonly RunPermissionsService _service;

        public RunPermissionsController(RunPermissionsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves the member permissions for a run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The run's member permissions.</returns>
        /// <remarks>
        /// Returns the current permission settings for the run.
        /// Permissions control what non-owner members are allowed to do:
        ///
        /// - Complete unassigned items (mark-complete)
        /// - Mark unassigned items as incomplete (mark-incomplete)
        /// - Change item descriptions (change-description)
        /// - Modify notes on unassigned items (update-notes)
        /// - Add new items (add items)
        /// - Remove items (delete items)
        ///
        /// Each permission is a boolean flag. Permissions apply to all non-owner
        /// members uniformly. The owner is not affected by permissions.
        ///
        /// The current user must be a member or owner of the run.
        ///
        /// Returns 403 Forbidden if the current user is not a member or owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(RunPermissionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<Result<RunPermissionsDto>> GetRunPermissionsAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            return await _service.GetRunPermissionsAsync(runId, cancellationToken);
        }

        /// <summary>
        /// Updates the member permissions for a run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The new permissions to apply.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated member permissions.</returns>
        /// <remarks>
        /// Completely replaces all member permissions for the run with the provided values.
        /// This is a full replacement — all permission fields are required and must be
        /// included in the request body. The request is rejected with a 400 Bad Request
        /// if any field is missing.
        ///
        /// Only the owner of the run can update permissions.
        ///
        /// Returns 400 Bad Request if the run is closed or any required field is missing.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpPut]
        [ProducesResponseType(typeof(RunPermissionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<Result<RunPermissionsDto>> UpdateRunPermissionsAsync([FromRoute] int runId, [FromBody] UpdateRunPermissionsDto dto, CancellationToken cancellationToken)
        {
            return await _service.UpdateRunPermissionsAsync(runId, dto, cancellationToken);
        }
    }
}