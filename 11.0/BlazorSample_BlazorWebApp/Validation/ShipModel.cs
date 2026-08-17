using System.ComponentModel.DataAnnotations;

namespace BlazorSample.Validation;

// Model for the static SSR client-side validation sample. The StartsWith attribute
// contributes a client-side rule enforced in the browser before the form is submitted.
public class ShipModel
{
    [Required]
    [StartsWith("NCC-")]
    public string? Registry { get; set; }
}
