using System.ComponentModel.DataAnnotations;
using TimeZoneConverter;

namespace UniTodo.Modules.Todos.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class ValidTimeZoneIdAttribute : ValidationAttribute
{
    private static readonly Lazy<HashSet<string>> ValidTimeZoneIds = new(() =>
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in TZConvert.KnownIanaTimeZoneNames)
            ids.Add(id);
        foreach (var id in TZConvert.KnownWindowsTimeZoneIds)
            ids.Add(id);
        return ids;
    });

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string id && ValidTimeZoneIds.Value.Contains(id))
            return ValidationResult.Success;

        return new ValidationResult($"The value '{value}' is not a valid time zone ID.");
    }
}
