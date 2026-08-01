using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Api.Filters;

namespace UniTodo.Modules.Todos.Api.Controllers
{
    [TypeFilter(typeof(ResultToActionResultFilter))]
    public abstract class TodoControllerBase : ControllerBase
    {
    }
}
