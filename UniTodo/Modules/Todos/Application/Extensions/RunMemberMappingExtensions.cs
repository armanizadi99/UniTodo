using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunMemberMappingExtensions
    {
        public static RunMemberDto ToDto(this RunMember member)
        {
            return new RunMemberDto(
    Id: member.Id,
                UserId: member.UserId.Value,
    CreatedAt: member.CreatedAt,
    UpdatedAt: member.UpdatedAt
            );
        }
    }
}