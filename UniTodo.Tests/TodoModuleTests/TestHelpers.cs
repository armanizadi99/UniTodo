using System.Reflection;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Tests.TodoModuleTests
{
    internal static class TestHelpers
    {
        public static void SetStatus(Run run, TodoListRunStatus status)
        {
            typeof(Run).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(run, status);
        }

        public static void SetId<T>(T entity, int id) where T : EntityBase<int>
        {
            typeof(EntityBase<int>).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entity, id);
        }

        public static void SetResetsAt(Run run, DateTimeOffset? resetsAt)
        {
            typeof(Run).GetField("<ResetsAt>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(run, resetsAt);
        }

        public static Run CreateActiveRun(string name = "Test Run", bool isShared = false, UserId? ownerId = null)
        {
            return new Run(name, ResetPolicy.Daily, isShared, ownerId ?? new UserId(Guid.NewGuid()));
        }

        public static UserId CreateUserId() => new UserId(Guid.NewGuid());
    }
}
