using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace BlazorWebAppRemoteValidation.Client.Models;

[ValidatableType]
public sealed class StarshipModel
{
    [Required]
    public string? Identifier { get; set; }

    [Required]
    public string? Classification { get; set; }

    public string? Description { get; set; }
}
