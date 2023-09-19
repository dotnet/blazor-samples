using System.ComponentModel.DataAnnotations;

namespace BlazorWebAppEFCore.Data;

public class Contact
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string? FirstName { get; set; }

    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string? LastName { get; set; }

    [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 digits.")]
    public string? Phone { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Street cannot exceed 100 characters.")]
    public string? Street { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
    public string? City { get; set; }

    [Required]
    [StringLength(3, ErrorMessage = "State abbreviation cannot exceed 3 characters.")]
    public string? State { get; set; }

    [Required]
    [RegularExpression(@"^\d{5}(?:[-\s]\d{4})?$", ErrorMessage = "Enter a valid zipcode in 55555 or 55555-5555 format")]
    public string? ZipCode { get; set; }
}
