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