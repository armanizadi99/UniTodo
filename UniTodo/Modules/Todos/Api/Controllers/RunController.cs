using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    [ApiController]
    [Route("api/runs")]
    [Authorize]
    public class RunController : ControllerBase
    {
        private readonly TodoListRunService _service;

        public RunController( TodoListRunService service )
        {
        _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUserActiveRunsAsync( CancellationToken cancellationToken )
        {
        var result = await _service.GetUserActiveTodoRunsAsync(cancellationToken);

        return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrivateEmptyRunAsync( [FromBody] CreateTodoListRunDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.CreateTodoListRunAsync(dto, cancellationToken);

        return Ok(result);
        }

        [HttpPost("CreateFromTemplate/{templateId:int:min(1)}")]
        public async Task<IActionResult> CreateRunFromTemplateAsync( [FromRoute] int templateId, CancellationToken cancellationToken )
        {
        var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, cancellationToken);

        return CreatedAtRoute("GetRunById", new { Id = result.Id }, result);
        }

        [HttpGet("{runId:int:min(1)}/items")]
        public async Task<IActionResult> GetRunItemsAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        var result = await _service.GetTodoListRunItemsAsync(runId, cancellationToken);

        return Ok(result);
        }

        [HttpGet("{runId:int:min(1)}", Name = "GetRunById")]
        public async Task<IActionResult> GetRunByIdAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        return Ok();
        }

        [HttpPost("{runId:int:min(1)}/items")]
        public async Task<IActionResult> AddItemToRunAsync( [FromRoute] int runId, [FromBody] AddTodoItemDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.AddTodoItemToTodoListRunAsync(runId, dto, cancellationToken);

        return Ok(result);
        }

        [HttpDelete("{runId:int:min(1)}/items/{itemId:int:min(1)}")]
        public async Task<IActionResult> DeleteItemFromRunAsync( [FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken )
        {
        await _service.DeleteTodoItemFromTodoListRunAsync(runId, itemId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/members")]
        public async Task<IActionResult> AdMemberToRunAsync( [FromRoute] int runId, [FromBody] AddMemberToTodoListRunDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.AddMemberToTodoListRunAsync(runId, dto, cancellationToken);

        return Ok(result);
        }

        [HttpDelete("{runId:int:min(1)}/members/remove/{userId:guid}")]
        public async Task<IActionResult> RemoveMemberFromRunAsync( [FromRoute] int runId, [FromRoute] Guid userId, CancellationToken cancellationToken )
        {
        await _service.RemoveMemberFromTodoListRunAsync(runId, userId, cancellationToken);

        return NoContent();
        }

        [HttpGet("{runId:int:min(1)}/members")]
        public async Task<IActionResult> GetRunMembersAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        var result = await _service.GetTodoListRunMembersAsync(runId, cancellationToken);

        return Ok(result);
        }

        [HttpPost("{runId:int:min(1)}/make-shared")]
        public async Task<IActionResult> MakeRunSharedAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        await _service.MakeTodoListRunSharedAsync(runId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/make-private")]
        public async Task<IActionResult> MakeRunPrivateAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        await _service.MakeTodoListRunPrivateAsync(runId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/items/{itemId:int:min(1)}/mark-complete")]
        public async Task<IActionResult> MarkRunItemCompleteAsync( [FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken )
        {
        await _service.MarkTodoItemCompleteAsync(runId, itemId, cancellationToken);

        return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/items/{itemId:int:min(1)}/mark-incomplete")]
        public async Task<IActionResult> MarkRunItemIncomplete( [FromRoute] int runId, [FromRoute] int itemId, CancellationToken cancellationToken )
        {
        await _service.MarkTodoItemIncompleteAsync(runId, itemId, cancellationToken);

        return NoContent();
        }

[HttpPatch("{runId:int:min(1)}/items/{itemId:int:min(1)}/update-notes")]
public async Task<IActionResult> UpdateItemNotesAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
{
        await _service.UpdateNotesForTodoItemAsync(runId, itemId, dto, cancellationToken);

        return NoContent();
        }

        [HttpPatch("{runId:int:min(1)}/items/{itemId:int:min(1)}/change-description")]
public async Task<IActionResult> ChangeRunItemDescriptionAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
{
        await _service.ChangeTodoItemDescriptionAsync(runId, itemId, dto, cancellationToken);

return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/items/{itemId:int:min(1)}/assign-to")]
public async Task<IActionResult> AssignRunItemToUserAsync([FromRoute] int runId, [FromRoute] int itemId, [FromBody] AssignTodoItemToMemberDto dto, CancellationToken cancellationToken)
{
        await _service.AssignItemToMemberAsync(runId, itemId, dto, cancellationToken);

        return NoContent();
        }
    }
}