namespace UniTodo.Modules.Todos.Domain.ValueObjects
{
    public readonly record struct RunSettings
    {
        public readonly TimeZoneInfo TimeZone { get; init; }
        public readonly DayOfWeek EndOfWeekDay { get; init; }
        public readonly bool PreserveHystory { get; init; }
    }
}
