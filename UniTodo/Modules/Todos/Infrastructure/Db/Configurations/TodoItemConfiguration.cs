using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
public void Configure(EntityTypeBuilder<TodoItem> builder)
{
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
        .HasConversion(id => id.Value,
        value => new TodoItemId(value));
        builder.Property(e => e.RunId)
        .HasConversion(id => id.Value,
        value => new Domain.ValueObjects.TodoListRunId(value))
.IsRequired();

        builder.Property(e => e.description)
        .HasConversion(description => description.Value,
        value => new Domain.ValueObjects.TodoItemDescription(value))
        .IsRequired()
        .HasMaxLength(Constants.DescriptionMaxLength);

        builder.Property(e => e.Notes)
        .HasConversion(notes => notes.Value,
        value => new Domain.ValueObjects.TodoItemNotes(value))
        .IsRequired()
        .HasMaxLength(Constants.NotesMaxLength);

        builder.Property(e => e.IsCompleted)
        .IsRequired();
        }
    }
}
