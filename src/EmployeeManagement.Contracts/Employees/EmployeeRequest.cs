using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Contracts.Validation;

namespace EmployeeManagement.Contracts.Employees;

// Body for both POST (create) and PUT (update); the employee id comes from the URL.
// Mutable class rather than a record so a Blazor form can bind to it directly.
public sealed class EmployeeRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name must be 100 characters or fewer.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, ErrorMessage = "Last name must be 100 characters or fewer.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    [StringLength(254, ErrorMessage = "Email address must be 254 characters or fewer.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(ValidationRules.PhonePattern, ErrorMessage = "Phone number must be in the format (XXX)-XXX-XXXX.")]
    public string? PhoneNumber { get; set; }

    // Nullable so an empty form field fails [Required] instead of defaulting to 0001-01-01.
    [Required(ErrorMessage = "Date of birth is required.")]
    [DateOfBirth]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    public AddressRequest? Address { get; set; } = new();
}