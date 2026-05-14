using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Infrastructure.Db.Configurations;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    internal class TodoDbContext : DbContext
    {

        public TodoDbContext( DbContextOptions<TodoDbContext> options ) 
        : base(options) { }

        internal DbSet<TodoListTemplate> todoLists { get; set; }
        internal DbSet<TodoItemTemplate> todoItemTemplates { get; set; }
        internal DbSet<TodoListRun> todoListRuns { get; set; }
        internal DbSet<TodoItem> todoItems { get; set; }
        protected override void OnModelCreating( ModelBuilder modelBuilder )
        {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TodoListTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TodoListRunConfiguration());
        modelBuilder.ApplyConfiguration(new TodoItemTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
        }
    }
}
