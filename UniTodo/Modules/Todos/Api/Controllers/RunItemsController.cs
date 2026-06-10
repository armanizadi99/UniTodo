using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
[ApiController]
[Route("/api/runs/{runId:int:min(1)}/items")]
    public class RunItemsController : ControllerBase
    {
        private readonly TodoListRunService _service;

public RunItemsController( TodoListRunService service )
        {
        _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetRunItemsAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        var result = await _service.GetTodoListRunItemsAsync(runId, cancellationToken);

        return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddItemToRunAsync( [FromRoute] int runId, [FromBody] AddTodoItemDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.AddTodoItemToTodoListRunAsync(runId, dto, cancellationToken);

        return Ok(result);
        }

        [HttpDelete("{itemId:int:min(1)}")]
        public async Task<IActionResult> DeleteItemFromRunAsync( [FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken )
        {
        var result = await _service.DeleteTodoItemFromTodoListRunAsync(runId, itemId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{itemId:int:min(1)}/mark-complete")]
        public async Task<IActionResult> MarkRunItemCompleteAsync( [FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken )
        {
        var result = await _service.MarkTodoItemCompleteAsync(runId, itemId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{itemId:int:min(1)}/mark-incomplete")]
        public async Task<IActionResult> MarkRunItemIncomplete( [FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken )
        {
        var result = await _service.MarkTodoItemIncompleteAsync(runId, itemId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{itemId:int:min(1)}/update-notes")]
        public async Task<IActionResult> UpdateItemNotesAsync( [FromRoute] int runId, [FromRoute] int itemId, [FromBody] UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.UpdateNotesForTodoItemAsync(runId, itemId, dto, cancellationToken);

        return NoContent();
        }

        [HttpPost("{itemId:int:min(1)}/change-description")]
        public async Task<IActionResult> ChangeRunItemDescriptionAsync( [FromRoute] int runId, [FromRoute] int itemId, [FromBody] ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.ChangeTodoItemDescriptionAsync(runId, itemId, dto, cancellationToken);

        return NoContent();
        }

        [HttpPost("{itemId:int:min(1)}/assign-to")]
        public async Task<IActionResult> AssignRunItemToUserAsync( [FromRoute] int runId, [FromRoute] int itemId, [FromBody] AssignTodoItemToMemberDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.AssignItemToMemberAsync(runId, itemId, dto, cancellationToken);

        return NoContent();
        }
    }
}
