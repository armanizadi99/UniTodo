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
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrivateEmptyRunAsync([FromBody] CreateTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("from-template/{templateId:int:min(1)}")]
        public async Task<IActionResult> CreateRunFromTemplateAsync([FromRoute] int templateId, CancellationToken cancellationToken)
        {
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return CreatedAtRoute("GetRunById", new { runId = result.Value.Id }, result.Value);
        }

        [HttpGet("{runId:int:min(1)}", Name = "GetRunById")]
        public async Task<IActionResult> GetRunByIdAsync( [FromRoute] int runId, CancellationToken cancellationToken )
        {
        return Ok();
        }

        [HttpPost("{runId:int:min(1)}/make-shared")]
        public async Task<IActionResult> MakeRunSharedAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeTodoListRunSharedAsync(runId, cancellationToken);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        [HttpPost("{runId:int:min(1)}/make-private")]
        public async Task<IActionResult> MakeRunPrivateAsync([FromRoute] int runId, CancellationToken cancellationToken)
        {
            var result = await _service.MakeTodoListRunPrivateAsync(runId, cancellationToken);
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