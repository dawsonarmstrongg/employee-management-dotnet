using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Contracts.Validation;

namespace EmployeeManagement.Contracts.Employees;

public sealed class AddressRequest
{
    [Required(ErrorMessage = "Address 1 is required.")]
    [StringLength(200, ErrorMessage = "Address 1 must be 200 characters or fewer.")]
    public string? Address1 { get; set; }

    [StringLength(200, ErrorMessage = "Address 2 must be 200 characters or fewer.")]
    public string? Address2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, ErrorMessage = "City must be 100 characters or fewer.")]
    public string? City { get; set; }

    [Required(ErrorMessage = "State is required.")]
    [UsState]
    public string? State { get; set; }

    [Required(ErrorMessage = "Zip is required.")]
    [RegularExpression(ValidationRules.ZipPattern, ErrorMessage = "Zip must be a 5-digit US ZIP code.")]
    public string? Zip { get; set; }
}