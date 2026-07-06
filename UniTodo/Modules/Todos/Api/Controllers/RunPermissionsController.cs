using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing a run's member permissions.
    /// </summary>
    [ApiController]
    [Route("api/runs/{runId:int:min(1)}/permissions")]
    [Authorize]
    public class RunPermissionsController : ControllerBase
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
        [HttpGet]
        [ProducesResponseType(typeof(RunPermissionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRunPermissionsAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetRunPermissionsAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Updates the member permissions for a run. Replaces all permissions with the provided values.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The new permissions to apply.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated member permissions.</returns>
        [HttpPut]
        [ProducesResponseType(typeof(RunPermissionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateRunPermissionsAsync([FromRoute] int runId, [FromBody] UpdateRunPermissionsDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateRunPermissionsAsync(runId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }
    }
}
