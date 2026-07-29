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
        /// <remarks>
        /// Returns all items in the run's current (active) iteration.
        /// Each item includes its description, completion status, assigned user,
        /// notes, and timestamps.
        ///
        /// The current user must be a member or owner of the run.
        ///
        /// Returns 403 Forbidden if the current user is not a member or owner.
        /// Returns 404 Not Found if the run does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Adds a new item to the run's current iteration. The current user must be
        /// a member or owner of the run. If the run is shared and the user is not
        /// the owner, the MemberAllowedToAddItems permission must be enabled.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user lacks permission.
        /// Returns 404 Not Found if the run does not exist.
        /// Returns 409 Conflict if an item with the same description already exists
        /// in the current iteration.
        /// </remarks>
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
        /// <remarks>
        /// Removes a single item from the run's current iteration.
        /// The current user must be a member or owner of the run.
        /// If the run is shared and the user is not the owner, the
        /// MemberAllowedToRemoveItems permission must be enabled.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user lacks permission.
        /// Returns 404 Not Found if the run or item does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Marks an item as completed. The item's completion timestamp and the
        /// completing user's ID are recorded.
        ///
        /// Permission rules:
        /// - The item assignee can always complete items assigned to them.
        /// - The owner can complete unassigned items.
        /// - Non-owner members can complete unassigned items only if the
        ///   MemberAllowedToCompleteUnassignedItems permission is enabled.
        ///
        /// Returns 400 Bad Request if the run is closed or the item is already completed.
        /// Returns 403 Forbidden if the current user lacks permission.
        /// Returns 404 Not Found if the run or item does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Reverts a completed item back to incomplete state.
        /// The item's completion timestamp and completing user ID are cleared.
        ///
        /// Permission rules:
        /// - The item assignee can always mark their items as incomplete.
        /// - The owner can mark unassigned items as incomplete.
        /// - Non-owner members can mark unassigned items as incomplete only if the
        ///   MemberAllowedToMarkIncompleteUnassignedItems permission is enabled.
        ///
        /// Returns 400 Bad Request if the run is closed or the item is already incomplete.
        /// Returns 403 Forbidden if the current user lacks permission.
        /// Returns 404 Not Found if the run or item does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Updates the free-text notes attached to a run item.
        /// Notes have a maximum length enforced by the domain model.
        ///
        /// Permission rules:
        /// - The item assignee and the run owner can always update notes.
        /// - Non-owner members can update notes on unassigned items only if the
        ///   MemberAllowedToModifyNotesForUnassignedItems permission is enabled.
        ///
        /// Returns 400 Bad Request if the run is closed or the notes exceed the maximum length.
        /// Returns 403 Forbidden if the current user lacks permission.
        /// Returns 404 Not Found if the run or item does not exist.
        /// </remarks>
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
        /// <remarks>
        /// Changes the description text of a run item.
        ///
        /// Permission rules:
        /// - The run owner can always change item descriptions.
        /// - Non-owner members can change descriptions only if the
        ///   MemberAllowedToChangeDescriptions permission is enabled.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user lacks permission.
        /// Returns 404 Not Found if the run or item does not exist.
        /// Returns 409 Conflict if the new description duplicates an existing
        /// item description in the same iteration.
        /// </remarks>
        [HttpPost("{itemId:int:min(1)}/change-description")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
        /// <remarks>
        /// Assigns a run item to a specific member of the run.
        /// The target user must already be a member of the run.
        /// Only the owner of the run can assign items to members.
        ///
        /// An item can be assigned to at most one member at a time.
        /// Assigning an already-assigned item to a different member replaces
        /// the previous assignment.
        ///
        /// Returns 400 Bad Request if the run is closed.
        /// Returns 403 Forbidden if the current user is not the owner.
        /// Returns 404 Not Found if the run, item, or target member does not exist.
        /// </remarks>
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