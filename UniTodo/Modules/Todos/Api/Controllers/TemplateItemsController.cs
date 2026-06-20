using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    /// <summary>
    /// Controller for managing todo item templates within a todo list template.
    /// </summary>
    [ApiController]
    [Route("api/templates/{todoListTemplateId:int:min(1)}/items")]
    [Authorize]
    public class TemplateItemsController : ControllerBase
    {
        private readonly TodoListTemplateItemsService _service;

        public TemplateItemsController(TodoListTemplateItemsService service)
        {
            _service = service;
        }

        /// <summary>
        /// Adds a new todo item template to a specific todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the parent todo list template.</param>
        /// <param name="dto">The data transfer object containing todo item template details.</param>
        /// <returns>The created todo item template.</returns>
        [HttpPost]
        public async Task<IActionResult> AddTodoItemTemplate([FromRoute] int todoListTemplateId, [FromBody] AddTodoItemTemplateDto dto)
        {
            var result = await _service.AddTodoItemTemplateAsync(todoListTemplateId, dto);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return Ok(result.Value);
        }

        /// <summary>
        /// Deletes a specific todo item template from a todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the parent todo list template.</param>
        /// <param name="todoItemTemplateId">The identifier of the todo item template to delete.</param>
        /// <returns>No content.</returns>
        [HttpDelete("{todoItemTemplateId:int}")]
        public async Task<IActionResult> DeleteTodoItemTemplateAsync([FromRoute] int todoListTemplateId, [FromRoute] int todoItemTemplateId)
        {
            var result = await _service.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId);
            if (!result.IsSuccess)
                return MapError(result.Error);

            return NoContent();
        }

        /// <summary>
        /// Retrieves all todo item templates associated with a specific todo list template.
        /// </summary>
        /// <param name="todoListTemplateId">The identifier of the todo list template.</param>
        /// <returns>A list of todo item templates for the specified todo list template.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTodoItemTemplatesAsync([FromRoute] int todoListTemplateId)
        {
            var result = await _service.GetTodoItemTemplatesAsync(todoListTemplateId);
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
