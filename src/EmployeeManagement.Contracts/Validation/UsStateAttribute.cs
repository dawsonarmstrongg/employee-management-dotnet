using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Contracts.Validation;

// Accepts the 50 state codes plus DC, case-insensitively. Null is left to [Required].
[AttributeUsage(AttributeTargets.Property)]
public sealed class UsStateAttribute : ValidationAttribute
{
    private static readonly HashSet<string> Codes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
        "DC",
    };

    public UsStateAttribute() : base("State must be a valid 2-letter US state code (for example, TX).") { }

    public static bool IsValidCode(string code) => Codes.Contains(code);

    public override bool IsValid(object? value) => value is not string code || IsValidCode(code);
}