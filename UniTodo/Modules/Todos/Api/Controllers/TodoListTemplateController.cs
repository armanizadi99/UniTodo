using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodoListTemplateController : ControllerBase
    {
        private readonly TodoListTemplateService _TodoListTemplateService;

        public TodoListTemplateController(TodoListTemplateService todoListTemplateService)
        {
            _TodoListTemplateService = todoListTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodoListTemplatesForCurrentUserAsync()
        {
            var result = await _TodoListTemplateService.GetUserTodoListsAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto)
        {
            var result = await _TodoListTemplateService.CreateTodoListTemplateAsync(dto);
            return CreatedAtRoute("GetTodoListTemplateById", new { id = result.Id }, result);
        }

        [HttpGet("{id:int:min(1)}", Name = "GetTodoListTemplateById")]
        public async Task<IActionResult> GetTodoListTemplateByIdAsync([FromRoute] int id)
        {
            var result = await _TodoListTemplateService.GetTodoListTemplateByIdAsync(id);

            return Ok(result);
        }

        [HttpDelete("{id:int:min(1)}")]
        public async Task<IActionResult> DeleteTodoListTemplate([FromRoute] int id)
        {
            await _TodoListTemplateService.DeleteTodoListAsync(id);

            return NoContent();
        }

        [HttpPost("{todoListTemplateId:int:min(1)}/AddTodoItemTemplate")]
        public async Task<IActionResult> AddTodoItemTemplate([FromRoute] int todoListTemplateId, [FromBody] AddTodoItemTemplateDto dto)
        {
            var result = await _TodoListTemplateService.AddTodoItemTemplateAsync(todoListTemplateId, dto);

            return Ok(result);
        }

        [HttpDelete("{TodoListTemplateId:int:min(1)}/DeleteTodoItemTemplate/{TodoItemTemplateId:int}")]
        public async Task<IActionResult> DeleteTodoItemTemplateAsync([FromRoute] int todoListTemplateId, [FromRoute] int todoItemTemplateId)
        {
            await _TodoListTemplateService.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId);

            return NoContent();
        }

        [HttpGet("{TodoListTemplateId:int:min(1)}/GetTodoItemTemplates")]
        public async Task<IActionResult> GetTodoItemTemplatesAsync([FromRoute] int todoListTemplateId)
        {
            var result = await _TodoListTemplateService.GetTodoItemTemplatesAsync(todoListTemplateId);

            return Ok(result);
        }
    }
}
