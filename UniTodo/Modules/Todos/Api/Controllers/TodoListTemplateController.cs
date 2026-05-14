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
    public  class TodoListTemplateController : ControllerBase
    {
        private readonly ITodoListTemplateService _TodoListTemplateService;

public  TodoListTemplateController(ITodoListTemplateService todoListTemplateService)
{
        _TodoListTemplateService = todoListTemplateService;
        }

[HttpGet]
public async Task<ActionResult<List<TodoListTemplateDto>>> GetAllTodoListTemplatesForCurrentUserAsync()
{
        var result = await _TodoListTemplateService.GetUserTodoListsAsync();

        return Ok(result);
        }

[HttpPost]
public async Task<ActionResult<int>> CreateTodoListTemplateAsync([FromBody] CreateTodoListTemplateDto dto)
{
        var result = await _TodoListTemplateService.CreateTodoListTemplateAsync(dto);

        return CreatedAtAction(nameof(GetTodoListTemplateByIdAsync), new { id = result.Id }, result);
        }

[HttpGet("{id:int:min(1)}")]
public async Task<ActionResult<TodoListTemplateDto>> GetTodoListTemplateByIdAsync([FromRoute] int id)
{
        var result = await _TodoListTemplateService.GetTodoListTemplateByIdAsync(id);

        return Ok(result);
        }
    }
}
