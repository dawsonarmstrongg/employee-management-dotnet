using System.Linq.Expressions;
using EmployeeManagement.Contracts.Employees;
using EmployeeManagement.Server.Data;

namespace EmployeeManagement.Server.Employees;

// Hand-written mapping between entities, requests and DTOs (no AutoMapper).
public static class EmployeeMapping
{
    // An expression rather than a method so EF Core can translate it into the SQL SELECT.
    public static readonly Expression<Func<Employee, EmployeeDto>> ToDto = e => new EmployeeDto(
        e.Id,
        e.FirstName,
        e.LastName,
        e.Email,
        e.PhoneNumber,
        e.DateOfBirth,
        new AddressDto(e.Address.Address1, e.Address.Address2, e.Address.City, e.Address.State, e.Address.Zip));

    private static readonly Func<Employee, EmployeeDto> ToDtoCompiled = ToDto.Compile();

    public static EmployeeDto ToDtoFrom(Employee employee) => ToDtoCompiled(employee);

    // Trims every field, uppercases State and turns a blank Address2 into null.
    // Runs before validation so "  " counts as missing and duplicate checks compare clean values.
    public static EmployeeRequest Normalize(EmployeeRequest request) => new()
    {
        FirstName = request.FirstName?.Trim(),
        LastName = request.LastName?.Trim(),
        Email = request.Email?.Trim(),
        PhoneNumber = request.PhoneNumber?.Trim(),
        DateOfBirth = request.DateOfBirth,
        Address = request.Address is null ? null : new AddressRequest
        {
            Address1 = request.Address.Address1?.Trim(),
            Address2 = string.IsNullOrWhiteSpace(request.Address.Address2) ? null : request.Address.Address2.Trim(),
            City = request.Address.City?.Trim(),
            State = request.Address.State?.Trim().ToUpperInvariant(),
            Zip = request.Address.Zip?.Trim(),
        },
    };

    // Copies a normalized, validated request onto an entity (new or existing).
    public static void Apply(EmployeeRequest request, Employee employee)
    {
        employee.FirstName = request.FirstName!;
        employee.LastName = request.LastName!;
        employee.Email = request.Email!;
        employee.PhoneNumber = request.PhoneNumber!;
        employee.DateOfBirth = request.DateOfBirth!.Value;

        employee.Address ??= new Address();
        employee.Address.Address1 = request.Address!.Address1!;
        employee.Address.Address2 = request.Address.Address2;
        employee.Address.City = request.Address.City!;
        employee.Address.State = request.Address.State!;
        employee.Address.Zip = request.Address.Zip!;
    }
}