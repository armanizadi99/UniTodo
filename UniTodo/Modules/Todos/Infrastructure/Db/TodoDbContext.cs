using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using UniTodo.Modules.Todos.Infrastructure.Db.Configurations;
using UniTodo.Modules.Todos.Infrastructure.Db.Converters;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    public class TodoDbContext : DbContext
    {

        public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options) { }

        public DbSet<TodoListTemplate> todoLists { get; set; }
        public DbSet<TodoItemTemplate> todoItemTemplates { get; set; }
        public DbSet<Run> runs { get; set; }
        public DbSet<RunIteration> runIterations { get; set; }
        public DbSet<RunItem> runItems { get; set; }
        public DbSet<RunMember> runMembers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TodoListTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new RunConfiguration());
            modelBuilder.ApplyConfiguration(new RunIterationConfiguration());
            modelBuilder.ApplyConfiguration(new TodoItemTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new RunItemConfiguration());
            modelBuilder.ApplyConfiguration(new RunMemberConfiguration());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder
            .Properties<UserId>()
            .HaveConversion<UserIdConverter>();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var entries = ChangeTracker.Entries<IAuditable>();

            foreach (var entry in entries)
            {
                var now = DateTimeOffset.UtcNow;

                if (entry.State is EntityState.Added)
                {
                    entry.Property(p => p.CreatedAt).CurrentValue = now;
                }
                if (entry.State is EntityState.Modified)
                {
                    entry.Property(p => p.UpdatedAt).CurrentValue = now;
                }
            }
            return await base.SaveChangesAsync(ct);
        }
    }
}