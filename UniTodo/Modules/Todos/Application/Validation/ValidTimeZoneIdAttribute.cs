using System.ComponentModel.DataAnnotations;

namespace UniTodo.Modules.Todos.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class ValidTimeZoneIdAttribute : ValidationAttribute
{
    private static readonly Lazy<HashSet<string>> ValidTimeZoneIds = new(() =>
        TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase));

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string id && ValidTimeZoneIds.Value.Contains(id))
            return ValidationResult.Success;

        return new ValidationResult($"The value '{value}' is not a valid time zone ID.");
    }
}
