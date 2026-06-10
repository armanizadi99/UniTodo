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

        public RunController(TodoListRunService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUserActiveRunsAsync(CancellationToken cancellationToken)
        {
            var result = await _service.GetUserActiveTodoRunsAsync(cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrivateEmptyRunAsync([FromBody] CreateTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunAsync(dto, cancellationToken);

            return Ok(result);
        }

        [HttpPost("from-template/{templateId:int:min(1)}")]
        public async Task<IActionResult> CreateRunFromTemplateAsync([FromRoute] int templateId, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, cancellationToken);

            return CreatedAtRoute("GetRunById", new { runId = result.Id }, result);
        }

        [HttpGet("{runId:int:min(1)}", Name = "GetRunById")]
        public async Task<IActionResult> GetRunByIdAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        return Ok();
        }

        [HttpPost("{runId:int:min(1)}/make-shared")]
        public async Task<IActionResult> MakeRunSharedAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            await _service.MakeTodoListRunSharedAsync(runId, cancellationToken);

            return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/make-private")]
        public async Task<IActionResult> MakeRunPrivateAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            await _service.MakeTodoListRunPrivateAsync(runId, cancellationToken);

            return NoContent();
        }
    }
}