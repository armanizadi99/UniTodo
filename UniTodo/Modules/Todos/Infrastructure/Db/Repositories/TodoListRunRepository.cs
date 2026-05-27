using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Repositories
{
    internal class TodoListRunRepository : Repository<TodoListRun>, ITodoListRunRepository
    {
        public TodoListRunRepository(TodoDbContext context) : base(context) { }
    }
}
