using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using UniTodo.Modules.Todos.Infrastructure.Db.Converters;

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

            builder.ComplexProperty(e => e.Settings, cfg =>
            {
                cfg.Property(s => s.TimeZone)
                    .HasConversion<TimeZoneConverter>()
                    .IsRequired();
                cfg.Property(s => s.EndOfWeekDay)
                    .IsRequired();
                cfg.Property(s => s.PreserveHystory)
                    .IsRequired();
            });

            builder.ComplexProperty(e => e.Permissions, cfg =>
            {
                cfg.Property(p => p.MemberAllowedToCompleteUnassignedItems)
                    .IsRequired();
                cfg.Property(p => p.MemberAllowedToChangeDescriptions)
                    .IsRequired();
                cfg.Property(p => p.MemberAllowedToAddItems)
                    .IsRequired();
                cfg.Property(p => p.MemberAllowdToRemoveItems)
                    .IsRequired();
                cfg.Property(p => p.MemberAllowedToMarkIncompleteUnassignedItems)
                .IsRequired();
                cfg.Property(p => p.MemberAllowedToModifyNotesForUnassignedItems)
                .IsRequired();
            });

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
