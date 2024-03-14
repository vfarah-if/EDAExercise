using System.Collections;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Attribute to validate there is at least one item
    /// </summary>
    /// <remarks>These custom validation attributes mimic the behavior of minItems in JSON Schema </remarks>
    /// <param name="minItems"></param>
    public class MinItemsAttribute(int minItems) : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value is ICollection collection && collection.Count < minItems)
            {
                return new ValidationResult($"The array must have at least {minItems} items.");
            }

            return ValidationResult.Success;
        }
    }
}
