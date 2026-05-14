using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    internal class TodoListTemplateConfiguration : IEntityTypeConfiguration<TodoListTemplate>
    {
void IEntityTypeConfiguration<TodoListTemplate>.Configure(EntityTypeBuilder<TodoListTemplate> builder)
{
        builder.Metadata.FindNavigation(nameof(TodoListTemplate.TodoItemTemplates))!
.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasKey(e => e.Id);


        builder.Property(e => e.OwnerId)
        .HasConversion(id => id.Value,
value => new Domain.ValueObjects.UserId(value))
.IsRequired();

        builder.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(Constants.NameMaxLength);

        builder.Property(e => e.ResetPolicy)
        .IsRequired();

        builder.Property(e => e.Status)
        .IsRequired();

        builder.HasMany(e => e.TodoItemTemplates)
        .WithOne(e => e.TodoList)
        .HasForeignKey(e => e.TodoListId);
        }
    }
}
