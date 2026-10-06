using EmployeeManagement.Contracts.Employees;
using EmployeeManagement.Contracts.Validation;
using EmployeeManagement.Server.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Server.Employees;

public sealed class EmployeeService(AppDbContext db) : IEmployeeService
{
    // SQLITE_CONSTRAINT: raised when the unique email index rejects a row.
    private const int SqliteConstraintError = 19;

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Employees
            .AsNoTracking()
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Select(EmployeeMapping.ToDto)
            .ToListAsync(cancellationToken);

    public Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Employees
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(EmployeeMapping.ToDto)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<EmployeeResult> CreateAsync(EmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = EmployeeRequestNormalizer.Normalize(request);
        var errors = EmployeeRequestValidator.Validate(normalized);
        if (errors.Count > 0)
        {
            return EmployeeResult.Invalid(errors);
        }

        if (await EmailInUseAsync(normalized.Email!, excludeId: null, cancellationToken))
        {
            return EmployeeResult.DuplicateEmail;
        }

        var employee = new Employee();
        EmployeeMapping.Apply(normalized, employee);
        db.Employees.Add(employee);

        return await SaveAsync(employee, cancellationToken);
    }

    public async Task<EmployeeResult> UpdateAsync(int id, EmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = EmployeeRequestNormalizer.Normalize(request);
        var errors = EmployeeRequestValidator.Validate(normalized);
        if (errors.Count > 0)
        {
            return EmployeeResult.Invalid(errors);
        }

        var employee = await db.Employees
            .Include(e => e.Address)
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is null)
        {
            return EmployeeResult.NotFound;
        }

        if (await EmailInUseAsync(normalized.Email!, excludeId: id, cancellationToken))
        {
            return EmployeeResult.DuplicateEmail;
        }

        EmployeeMapping.Apply(normalized, employee);
        return await SaveAsync(employee, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Single DELETE statement; the database's ON DELETE CASCADE removes the address.
        var deleted = await db.Employees
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        return deleted > 0;
    }

    // The Email column uses NOCASE collation, so this comparison is case-insensitive in SQL.
    private Task<bool> EmailInUseAsync(string email, int? excludeId, CancellationToken cancellationToken) =>
        db.Employees.AnyAsync(e => e.Email == email && e.Id != excludeId, cancellationToken);

    private async Task<EmployeeResult> SaveAsync(Employee employee, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintError })
        {
            // Two requests with the same email can both pass EmailInUseAsync; the unique index is the final guard.
            return EmployeeResult.DuplicateEmail;
        }

        return EmployeeResult.Success(EmployeeMapping.ToDtoFrom(employee));
    }
}