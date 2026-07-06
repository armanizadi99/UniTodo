using System.ComponentModel.DataAnnotations;
using UniTodo.Modules.Todos.Application.Validation;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Request DTO for updating run settings.</summary>
    public class UpdateRunSettingsDto
    {
        /// <summary>The IANA or Windows time zone identifier for scheduling resets.</summary>
        [Required]
        [ValidTimeZoneId]
        public required string? TimeZone { get; set; }

        /// <summary>The last day of the week for weekly reset calculations.</summary>
        [Required]
        [EnumDataType(typeof(DayOfWeek))]
        public required DayOfWeek? EndOfWeekDay { get; set; }

        /// <summary>Whether historical iterations are preserved after a reset.</summary>
        [Required]
        public required bool? PreserveHystory { get; set; }
    }
}
