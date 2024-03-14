using System.ComponentModel.DataAnnotations;

namespace Domain.Extensions;

public static class DataAnnotationValidationExtension
{
    public static bool IsValid<T>(this T source, out ICollection<ValidationResult> errors)
    {
        var validationResults = new List<ValidationResult>();
        if (source == null)
        {
            errors = validationResults;
            return false;
        }
        var validationContext = new ValidationContext(source);
        var result = Validator.TryValidateObject(
            source,
            validationContext,
            validationResults,
            true
        );
        errors = validationResults;
        return result;
    }
}
