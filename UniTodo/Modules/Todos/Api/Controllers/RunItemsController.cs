using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Extensions;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing run items within a run's current iteration.
    /// </summary>
    [ApiController]
    [Route("/api/runs/{runId:int:min(1)}/items")]
    [Authorize]
    public class RunItemsController : ControllerBase
    {
        private readonly RunItemsService _service;

        public RunItemsController(RunItemsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all run items for a specific run's current iteration.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of run items for the specified run.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<RunItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRunItemsAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetRunItemsAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Adds a new run item to a specific run's current iteration.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="dto">The data transfer object containing run item details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created run item.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RunItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddItemToRunAsync([FromRoute] int runId, [FromBody] AddRunItemDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.AddRunItemToRunAsync(runId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a specific run item from a run's current iteration.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="itemId">The identifier of the run item to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{itemId:int:min(1)}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteItemFromRunAsync([FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteRunItemFromRunAsync(runId, itemId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Marks a specific run item as complete.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="itemId">The identifier of the run item to mark as complete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/mark-complete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkRunItemCompleteAsync([FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken)
        {
            var result = await _service.MarkRunItemCompleteAsync(runId, itemId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Marks a specific run item as incomplete.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="itemId">The identifier of the run item to mark as incomplete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/mark-incomplete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> MarkRunItemIncomplete([FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken)
        {
            var result = await _service.MarkRunItemIncompleteAsync(runId, itemId, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Updates the notes for a specific run item.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="itemId">The identifier of the run item.</param>
        /// <param name="dto">The data transfer object containing the updated notes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/update-notes")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateItemNotesAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] UpdateNotesForRunItemDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateNotesForRunItemAsync(runId, itemId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Changes the description of a specific run item.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="itemId">The identifier of the run item.</param>
        /// <param name="dto">The data transfer object containing the new description.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/change-description")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeRunItemDescriptionAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] ChangeRunItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.ChangeRunItemDescriptionAsync(runId, itemId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }

        /// <summary>
        /// Assigns a specific run item to a member of the run.
        /// </summary>
        /// <param name="runId">The identifier of the run.</param>
        /// <param name="itemId">The identifier of the run item.</param>
        /// <param name="dto">The data transfer object containing the member assignment details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/assign-to")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AssignRunItemToUserAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] AssignRunItemToMemberDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.AssignItemToMemberAsync(runId, itemId, dto, cancellationToken);
            if (!result.IsSuccess)
                return result.Error.ToActionResult();

            return NoContent();
        }
    }
}
