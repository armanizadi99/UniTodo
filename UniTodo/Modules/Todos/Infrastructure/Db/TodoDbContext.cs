using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Infrastructure.Db.Configurations;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    public class TodoDbContext : DbContext
    {

        public TodoDbContext( DbContextOptions<TodoDbContext> options ) 
        : base(options) { }

        public DbSet<TodoListTemplate> todoLists { get; set; }
        public DbSet<TodoItemTemplate> todoItemTemplates { get; set; }
        public DbSet<TodoListRun> todoListRuns { get; set; }
        public DbSet<TodoItem> todoItems { get; set; }
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
