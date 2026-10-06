namespace EmployeeManagement.Contracts.Validation;

public static class ValidationRules
{
    // [0-9] rather than \d: in .NET, \d also matches non-ASCII digits such as Arabic-Indic numerals.
    public const string PhonePattern = @"^\([0-9]{3}\)-[0-9]{3}-[0-9]{4}$";
    public const string ZipPattern = @"^[0-9]{5}$";

    public static readonly DateOnly EarliestDateOfBirth = new(1900, 1, 1);
}