using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunMemberMappingExtensions
    {
public static TodoListRunMemberDto ToDto(this RunMember member)
{
        return new TodoListRunMemberDto(
Id: member.Id,
            UserId: member.UserId.Value,
CreatedAt: member.CreatedAt,
UpdatedAt: member.UpdatedAt
        );
        }
    }
}