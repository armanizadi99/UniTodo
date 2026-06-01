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

            builder.Property(e => e.IsShared)
            .IsRequired();

            builder.Property(e => e.ClosedAt);

            builder.Property(e => e.Members)
                .HasConversion(
                    v => JsonSerializer.Serialize(v.Select(m => m.Value), (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null!)!
                        .Select(g => new UserId(g)).ToList(),
                    new ValueComparer<IReadOnlyCollection<UserId>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => (IReadOnlyCollection<UserId>)c.ToList()))
                .HasColumnName("Members")
                .HasColumnType("TEXT");

            builder.HasMany(e => e.TodoItems)
            .WithOne(e => e.Run)
            .HasForeignKey(e => e.RunId);

            builder.Navigation(nameof(TodoListRun.TodoItems))
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
