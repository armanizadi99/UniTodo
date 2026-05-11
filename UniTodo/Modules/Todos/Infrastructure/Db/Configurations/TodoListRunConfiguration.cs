using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    public class TodoListRunConfiguration : IEntityTypeConfiguration<TodoListRun>
    {
        public void Configure( EntityTypeBuilder<TodoListRun> builder )
{
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
        .HasConversion(id => id.Value,
        value => new Domain.ValueObjects.TodoListRunId(value));

        builder.Property(e => e.ownerId)
        .HasConversion(id => id.Value,
        value => new Domain.ValueObjects.UserId(value))
.IsRequired();

        builder.Property(e => e.ResetPolicy)
        .IsRequired();

        builder.Property(e => e.Status)
        .IsRequired();

        builder.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(100);


        builder.Metadata.FindNavigation(nameof(TodoListRun.TodoItems))!
        .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.TodoItems)
        .WithOne(e => e.Run)
        .HasForeignKey(e => e.RunId);
        }
    }
}
