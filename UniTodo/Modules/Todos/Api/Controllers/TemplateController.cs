using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    [ApiController]
    [Route("api/templates")]
    [Authorize]
    public class TemplateController : ControllerBase
    {
        private readonly TodoListTemplateService _TodoListTemplateService;

        public TemplateController(TodoListTemplateService todoListTemplateService)
        {
            _TodoListTemplateService = todoListTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodoListTemplatesForCurrentUserAsync()
        {
            var result = await _TodoListTemplateService.GetUserTodoListsAsync();
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto)
        {
            var result = await _TodoListTemplateService.CreateTodoListTemplateAsync(dto);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return CreatedAtRoute("GetTodoListTemplateById", new { id = result.Value.Id }, result.Value);
        }

        [HttpGet("{id:int:min(1)}", Name = "GetTodoListTemplateById")]
        public async Task<IActionResult> GetTodoListTemplateByIdAsync([FromRoute] int id)
        {
            var result = await _TodoListTemplateService.GetTodoListTemplateByIdAsync(id);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> DeleteTodoListTemplate([FromRoute] int id)
        {
            var result = await _TodoListTemplateService.DeleteTodoListAsync(id);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        [HttpPost("{todoListTemplateId:int:min(1)}/item-templates/")]
        public async Task<IActionResult> AddTodoItemTemplate([FromRoute] int todoListTemplateId, [FromBody] AddTodoItemTemplateDto dto)
        {
            var result = await _TodoListTemplateService.AddTodoItemTemplateAsync(todoListTemplateId, dto);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        [HttpDelete("{todoListTemplateId:int:min(1)}/template-items/{todoItemTemplateId:int}")]
        public async Task<IActionResult> DeleteTodoItemTemplateAsync([FromRoute] int todoListTemplateId, [FromRoute] int todoItemTemplateId)
        {
            var result = await _TodoListTemplateService.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        [HttpGet("{todoListTemplateId:int:min(1)}/item-templates")]
        public async Task<IActionResult> GetTodoItemTemplatesAsync([FromRoute] int todoListTemplateId)
        {
            var result = await _TodoListTemplateService.GetTodoItemTemplatesAsync(todoListTemplateId);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
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
