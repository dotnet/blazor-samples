using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorSample.Validation;

// A custom validation attribute that also contributes a client-side rule for static SSR
// forms. The rule name ('startswith') must match the validator registered on the client
// with Blazor.formValidation.addValidator.
public sealed class StartsWithAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    private readonly string prefix;

    public StartsWithAttribute(string prefix)
    {
        this.prefix = prefix;
        ErrorMessage = $"The value must start with '{prefix}'.";
    }

    protected override ValidationResult? IsValid(object? value,
        ValidationContext validationContext)
    {
        if (value is string text && !text.StartsWith(prefix, StringComparison.Ordinal))
        {
            return new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
        }

        return ValidationResult.Success;
    }

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(
            "startswith",
            new Dictionary<string, string> { ["prefix"] = prefix });
    }
}
