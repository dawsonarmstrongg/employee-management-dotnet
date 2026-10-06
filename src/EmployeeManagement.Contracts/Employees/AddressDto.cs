namespace EmployeeManagement.Contracts.Employees;

public sealed record AddressDto(
    string Address1,
    string? Address2,
    string City,
    string State,
    string Zip);