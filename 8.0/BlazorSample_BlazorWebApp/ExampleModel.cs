using System.ComponentModel.DataAnnotations;

namespace BlazorSample;

public class ExampleModel
{
    [Required]
    [StringLength(10, ErrorMessage = "Name is too long.")]
    public string? Name { get; set; }
}
