using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Contracts.Validation;

// Rejects future dates and implausibly old ones. Null is left to [Required].
[AttributeUsage(AttributeTargets.Property)]
public sealed class DateOfBirthAttribute : ValidationAttribute
{
    public DateOfBirthAttribute() : base("Date of birth must be between 01/01/1900 and today.") { }

    public override bool IsValid(object? value) =>
        value is not DateOnly date
        || (date >= ValidationRules.EarliestDateOfBirth && date <= DateOnly.FromDateTime(DateTime.Today));
}