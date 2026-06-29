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
