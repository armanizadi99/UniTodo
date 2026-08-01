namespace UniTodo.Modules.Todos.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CreatedAtRouteResultAttribute : Attribute
    {
        public CreatedAtRouteResultAttribute(string routeName)
        {
            RouteName = routeName;
        }

        public string RouteName { get; }

        public string RouteValueName { get; set; } = "id";
    }
}
