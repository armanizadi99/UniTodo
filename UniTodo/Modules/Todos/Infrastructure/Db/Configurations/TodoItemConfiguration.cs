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
            .HasConversion(notes => notes == null ? null : notes.Value,
            value => value == null ? null : new Domain.ValueObjects.TodoItemNotes(value))
            .HasMaxLength(Constants.NotesMaxLength);

            builder.Property(e => e.AsignedTo)
            .HasConversion(id => id.HasValue ? (Guid?)id.Value.Value : (Guid?)null,
            value => value.HasValue ? new Domain.ValueObjects.UserId(value.Value) : (Domain.ValueObjects.UserId?)null);

            builder.Property(e => e.IsCompleted)
            .IsRequired();

            builder.Property(e => e.CompletedAt);

        builder.Property(e => e.CompletedBy)
.HasConversion(id => id.HasValue ? (Guid?)id.Value.Value : (Guid?)null,
            value => value.HasValue ? new Domain.ValueObjects.UserId(value.Value) : (Domain.ValueObjects.UserId?)null);
        }
    }
}
