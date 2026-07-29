using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing members of a run.
    /// </summary>
    [ApiController]
    [Route("api/runs/{runId:int:min(1)}/members")]
    [Authorize]
    public class RunMembersController : ControllerBase
    {
        private readonly RunMembersService _service;

        public RunMembersController(RunMembersService service)
        {
            _service = service;
        }

        /// <summary>
        /// Adds a new member to a specific run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The data transfer object containing member details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The added member details.</returns>
        /// <remarks>
        /// Adds a user as a member of the run. The run must be shared before
        /// members can be added (see the make-shared endpoint).
        ///
        /// Only the owner of the run can add members. The target user must exist.
        /// The owner themselves cannot be added as a member (they are already
        /// the owner).
        ///
        /// Returns 400 Bad Request if the run is closed or not shared.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run does not exist.
        /// Returns 409 Conflict if the user is already a member of the run.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(RunMemberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddMemberToRunAsync([FromRoute] int runId, [FromBody] AddMemberToRunDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.AddMemberToRunAsync(runId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Removes a member from a specific run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="userId">The identifier of the user to remove.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        /// <remarks>
        /// Removes a user from the run's member list.
        /// Only the owner of the run can remove members. The owner cannot
        /// remove themselves.
        ///
        /// The removed member loses access to the run and its items immediately.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run or member does not exist.
        /// </remarks>
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveMemberFromRunAsync([FromRoute] int runId, [FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var result = await _service.RemoveMemberFromRunAsync(runId, userId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Retrieves all members of a specific run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of members in the specified run.</returns>
        /// <remarks>
        /// Returns all members of the run, including their user ID and membership status.
        /// The run owner is not included in the member list — the owner has
        /// full control and is not treated as a regular member.
        ///
        /// The current user must be a member or owner of the run.
        ///
        /// Returns 403 Forbidden if the current user is not a member or owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RunMemberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRunMembersAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetRunMembersAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }
    }
}