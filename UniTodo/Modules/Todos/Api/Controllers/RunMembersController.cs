using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
[ApiController]
[Route("api/runs/{runId:int:min(1)}/members")]
[Authorize]
    public class RunMembersController : ControllerBase
    {
        private readonly TodoListRunService _service;

public RunMembersController( TodoListRunService service )
        {
        _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> AddMemberToRunAsync( [FromRoute] int runId, [FromBody] AddMemberToTodoListRunDto dto, CancellationToken cancellationToken )
        {
        var result = await _service.AddMemberToTodoListRunAsync(runId, dto, cancellationToken);

        return Ok(result);
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> RemoveMemberFromRunAsync( [FromRoute] int runId, [FromRoute] Guid userId, CancellationToken cancellationToken )
        {
        var result = await _service.RemoveMemberFromTodoListRunAsync(runId, userId, cancellationToken);

        return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetRunMembersAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        var result = await _service.GetTodoListRunMembersAsync(runId, cancellationToken);

        return Ok(result);
        }
    }
}
