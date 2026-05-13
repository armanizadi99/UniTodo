using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    public class TodoItemTemplateConfiguration : IEntityTypeConfiguration<TodoItemTemplate>
    {
public void Configure(EntityTypeBuilder<TodoItemTemplate> builder)
{
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TodoListId)
.IsRequired();

        builder.Property(e => e.Description)
        .HasConversion(description => description.Value,
        value => new Domain.ValueObjects.TodoItemDescription(value))
.IsRequired()
.HasMaxLength(Constants.DescriptionMaxLength);
        }
    }
}
