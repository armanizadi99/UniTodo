using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    internal class TodoListRunConfiguration : IEntityTypeConfiguration<TodoListRun>
    {
        void IEntityTypeConfiguration<TodoListRun>.Configure(EntityTypeBuilder<TodoListRun> builder)
        {
            builder.HasKey(e => e.Id);

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

            builder.Property(e => e.RunId)
            .IsRequired();

            builder.HasIndex(e => e.RunId).IsUnique(false);

            builder.Property(e => e.IsShared)
            .IsRequired();

            builder.Property(e => e.ClosedAt);


            builder.HasMany(e => e.TodoItems)
            .WithOne(e => e.Run)
            .HasForeignKey(e => e.RunId);

            builder.HasMany(e => e.Members)
            .WithOne(e => e.Run)
            .HasForeignKey(e => e.TodoListRunId)
            .HasPrincipalKey(e => e.RunId);

            builder.Navigation(nameof(TodoListRun.TodoItems))
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(nameof(TodoListRun.Members))
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
