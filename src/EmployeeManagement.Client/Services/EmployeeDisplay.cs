using System.Globalization;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Client.Services;

// Formatting for the employee table, kept out of the .razor file so it is easy to unit test.
public static class EmployeeDisplay
{
    public static string FullName(EmployeeDto employee) => $"{employee.FirstName} {employee.LastName}";

    // "Doe, Jane": the table sorts by last name, so it shows the last name first.
    public static string SortableName(EmployeeDto employee) => $"{employee.LastName}, {employee.FirstName}";

    // "123 Main St, Apt 4B, Springfield, IL 62701"; Address 2 is skipped when empty.
    public static string FullAddress(AddressDto address) => string.Join(", ",
        new[] { address.Address1, address.Address2, address.City, $"{address.State} {address.Zip}" }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

    public static string DateOfBirth(DateOnly dateOfBirth) =>
        dateOfBirth.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
}