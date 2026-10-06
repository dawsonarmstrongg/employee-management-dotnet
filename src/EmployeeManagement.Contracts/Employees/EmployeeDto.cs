namespace EmployeeManagement.Contracts.Employees;

// Returned by GET, POST and PUT. Immutable: the client only reads it.
public sealed record EmployeeDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    AddressDto Address);