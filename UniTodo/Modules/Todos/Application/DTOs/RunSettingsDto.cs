namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Settings that control run scheduling and reset behavior.</summary>
    public record RunSettingsDto
    {
        /// <summary>The IANA or Windows time zone identifier used for scheduling resets.</summary>
        public string TimeZone { get; init; } = null!;

        /// <summary>The last day of the week for weekly reset calculations.</summary>
        public DayOfWeek EndOfWeekDay { get; init; }

        /// <summary>Whether historical (closed) iterations are preserved after a reset.</summary>
        public bool PreserveHistory { get; init; }
    }
}
