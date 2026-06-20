using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing todo items within a todo list run.
    /// </summary>
    [ApiController]
    [Route("/api/runs/{runId:int:min(1)}/items")]
    [Authorize]
    public class RunItemsController : ControllerBase
    {
        private readonly TodoListRunService _service;

        public RunItemsController(TodoListRunService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all todo items for a specific todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of todo items for the specified run.</returns>
        [HttpGet]
        public async Task<IActionResult> GetRunItemsAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.GetTodoListRunItemsAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Adds a new todo item to a specific todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="dto">The data transfer object containing todo item details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created todo item.</returns>
        [HttpPost]
        public async Task<IActionResult> AddItemToRunAsync([FromRoute] int runId, [FromBody] AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.AddTodoItemToTodoListRunAsync(runId, dto, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a specific todo item from a todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="itemId">The identifier of the todo item to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{itemId:int:min(1)}")]
        public async Task<IActionResult> DeleteItemFromRunAsync([FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteTodoItemFromTodoListRunAsync(runId, itemId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Marks a specific todo item as complete in a todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="itemId">The identifier of the todo item to mark as complete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/mark-complete")]
        public async Task<IActionResult> MarkRunItemCompleteAsync([FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken)
        {
            var result = await _service.MarkTodoItemCompleteAsync(runId, itemId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Marks a specific todo item as incomplete in a todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="itemId">The identifier of the todo item to mark as incomplete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/mark-incomplete")]
        public async Task<IActionResult> MarkRunItemIncomplete([FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken)
        {
            var result = await _service.MarkTodoItemIncompleteAsync(runId, itemId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Updates the notes for a specific todo item in a todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="itemId">The identifier of the todo item.</param>
        /// <param name="dto">The data transfer object containing the updated notes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/update-notes")]
        public async Task<IActionResult> UpdateItemNotesAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateNotesForTodoItemAsync(runId, itemId, dto, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Changes the description of a specific todo item in a todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="itemId">The identifier of the todo item.</param>
        /// <param name="dto">The data transfer object containing the new description.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/change-description")]
        public async Task<IActionResult> ChangeRunItemDescriptionAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.ChangeTodoItemDescriptionAsync(runId, itemId, dto, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Assigns a specific todo item to a member of the todo list run.
        /// </summary>
        /// <param name="runId">The identifier of the todo list run.</param>
        /// <param name="itemId">The identifier of the todo item.</param>
        /// <param name="dto">The data transfer object containing the member assignment details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>No content.</returns>
        [HttpPost("{itemId:int:min(1)}/assign-to")]
        public async Task<IActionResult> AssignRunItemToUserAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] AssignTodoItemToMemberDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.AssignItemToMemberAsync(runId, itemId, dto, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        private IActionResult MapError(UniTodo.Modules.Todos.Domain.Common.DomainError error)
        {
            return error.Code switch
            {
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.EntityNotFound => NotFound(error.Message),
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.NotAuthorized => Forbid(),
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.InvalidOperation => BadRequest(error.Message),
                UniTodo.Modules.Todos.Domain.Common.DomainErrorCodes.DuplicateEntities => Conflict(error.Message),
                _ => StatusCode(500, "An unexpected error occurred.")
            };
        }
    }
}
