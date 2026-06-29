using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    internal class RunConfiguration : IEntityTypeConfiguration<Run>
    {
        void IEntityTypeConfiguration<Run>.Configure(EntityTypeBuilder<Run> builder)
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

            builder.Property(e => e.IsShared)
            .IsRequired();

            builder.Property(e => e.ClosedAt);

            builder.HasMany(e => e.Iterations)
            .WithOne(e => e.Run)
            .HasForeignKey("RunId")
            .IsRequired();

            builder.HasMany(e => e.Members)
            .WithOne(e => e.Run)
            .HasForeignKey(e => e.RunId);

            builder.Navigation(nameof(Run.Iterations))
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(nameof(Run.Members))
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
