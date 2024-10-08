using System.ComponentModel.DataAnnotations;

namespace BlazorSample;

public class SaladChefValidatorAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value,
        ValidationContext validationContext)
    {
        var saladChef = validationContext.GetRequiredService<SaladChef>();

        if (saladChef.SaladToppers.Contains(value?.ToString()))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("Is that a Vulcan salad topper?! " +
            "The following toppers are available for a Ten Forward salad: " +
            string.Join(", ", saladChef.SaladToppers));
    }
}
