using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Server.Employees;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeResult> CreateAsync(EmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResult> UpdateAsync(int id, EmployeeRequest request, CancellationToken cancellationToken = default);

    /// <returns>False when no employee has that id.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}