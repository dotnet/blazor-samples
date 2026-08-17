using System.ComponentModel.DataAnnotations;

namespace BlazorSample.Validation;

public class RegistrationModel
{
    [Required]
    public string Username { get; set; } = string.Empty;
}
