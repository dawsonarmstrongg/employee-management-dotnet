using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Contracts.Validation;

// Accepts the 50 state codes plus DC, case-insensitively. Null is left to [Required].
[AttributeUsage(AttributeTargets.Property)]
public sealed class UsStateAttribute : ValidationAttribute
{
    public UsStateAttribute() : base("State must be a valid 2-letter US state code (for example, TX).") { }

    public static bool IsValidCode(string code) => UsStates.IsValidCode(code);

    public override bool IsValid(object? value) => value is not string code || IsValidCode(code);
}