using System.Collections;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Validates that there is a unique list of Items
/// </summary>
/// <remarks>These custom validation attributes mimic the behavior of uniqueItems in JSON Schema </remarks>
public class UniqueItemsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable enumerable)
            return ValidationResult.Success;
        var hashSet = new HashSet<object>();

        if (enumerable.Cast<object?>().Any(item => item != null && !hashSet.Add(item)))
        {
            return new ValidationResult("All items in the array must be unique.");
        }

        return ValidationResult.Success;
    }
}
