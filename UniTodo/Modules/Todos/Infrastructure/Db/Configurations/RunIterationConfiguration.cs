using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    internal class RunIterationConfiguration : IEntityTypeConfiguration<RunIteration>
    {
        void IEntityTypeConfiguration<RunIteration>.Configure(EntityTypeBuilder<RunIteration> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Navigation(nameof(RunIteration.RunItems))
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
