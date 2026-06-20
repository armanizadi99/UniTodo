using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Configurations
{
    public class RunMemberConfiguration : IEntityTypeConfiguration<RunMember>
    {
        void IEntityTypeConfiguration<RunMember>.Configure(EntityTypeBuilder<RunMember> builder)
        {
            builder.Ignore(e => e.Id);
            builder.HasKey(e => new { e.UserId, e.RunId });
        }
    }
}
