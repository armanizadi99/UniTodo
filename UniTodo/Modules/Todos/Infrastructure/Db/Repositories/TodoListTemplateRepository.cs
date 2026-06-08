using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Repositories
{
    internal class TodoListTemplateRepository : Repository<TodoListTemplate>, ITodoListTemplateRepository
    {
        private readonly DbSet<TodoListTemplate> _dbSet;

        public TodoListTemplateRepository(TodoDbContext context) : base(context)
        {
            _dbSet = context.Set<TodoListTemplate>();
        }

        async Task<bool> ITodoListTemplateRepository.IsNameDuplicateAsync(string name, CancellationToken cancellationToken)
        {
            return await _dbSet.AnyAsync(e => e.Name == name, cancellationToken);
        }

        async Task<List<TodoListTemplate>> ITodoListTemplateRepository.GetUserTodoListTemplatesAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(e => e.OwnerId     == new UserId(userId)).ToListAsync();
        }

        async Task<TodoListTemplate?> ITodoListTemplateRepository.GetTodoListTemplateByIdAsync(int id, bool includeTodoItemTemplates, CancellationToken cancellationToken)
        {
            IQueryable<TodoListTemplate> query = _dbSet;

            if (includeTodoItemTemplates)
                query = query.Include(i => i.TodoItemTemplates);

            return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
    }
}
