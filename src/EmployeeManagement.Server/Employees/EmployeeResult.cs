using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Server.Employees;

public enum EmployeeOutcome
{
    Success,
    ValidationFailed,
    NotFound,
    DuplicateEmail,
}

// What a create or update produced. The service knows nothing about HTTP;
// the endpoints translate each outcome into a status code.
public sealed record EmployeeResult(
    EmployeeOutcome Outcome,
    EmployeeDto? Employee = null,
    IDictionary<string, string[]>? Errors = null)
{
    public static EmployeeResult Success(EmployeeDto employee) => new(EmployeeOutcome.Success, employee);
    public static EmployeeResult Invalid(IDictionary<string, string[]> errors) => new(EmployeeOutcome.ValidationFailed, Errors: errors);
    public static readonly EmployeeResult NotFound = new(EmployeeOutcome.NotFound);
    public static readonly EmployeeResult DuplicateEmail = new(EmployeeOutcome.DuplicateEmail);
}