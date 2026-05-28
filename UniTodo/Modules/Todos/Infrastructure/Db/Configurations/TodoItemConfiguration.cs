using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    internal class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
        void IEntityTypeConfiguration<TodoItem>.Configure(EntityTypeBuilder<TodoItem> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.RunId)
    .IsRequired();

            builder.Property(e => e.Description)
            .HasConversion(description => description.Value,
            value => new Domain.ValueObjects.TodoItemDescription(value))
            .IsRequired()
            .HasMaxLength(Constants.DescriptionMaxLength);

            builder.Property(e => e.Notes)
            .HasConversion(notes => notes!.Value,
            value => new Domain.ValueObjects.TodoItemNotes(value))
            .IsRequired()
            .HasMaxLength(Constants.NotesMaxLength);

            builder.Property(e => e.IsCompleted)
            .IsRequired();
        }
    }
}
