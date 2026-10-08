using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Client.Services;

public enum EmployeeSortColumn
{
    Name,
    Email,
    DateOfBirth,
}

public enum SortDirection
{
    Ascending,
    Descending,
}

public readonly record struct EmployeeSort(EmployeeSortColumn Column, SortDirection Direction)
{
    // Same order the API returns: last name, then first name.
    public static EmployeeSort Default => new(EmployeeSortColumn.Name, SortDirection.Ascending);

    // Clicking the sorted column flips its direction; clicking another column sorts it ascending.
    public EmployeeSort Toggle(EmployeeSortColumn column) => column == Column
        ? this with { Direction = Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending }
        : new EmployeeSort(column, SortDirection.Ascending);
}

// Client-side sorting for the employee table. The list is small and already loaded, so there is no
// need for a round trip to the API.
public static class EmployeeSorter
{
    public static List<EmployeeDto> Sort(IEnumerable<EmployeeDto> employees, EmployeeSort sort)
    {
        var text = StringComparer.OrdinalIgnoreCase;
        var descending = sort.Direction == SortDirection.Descending;

        var ordered = sort.Column switch
        {
            EmployeeSortColumn.Email => By(employees, e => e.Email, text, descending),
            EmployeeSortColumn.DateOfBirth => By(employees, e => e.DateOfBirth, Comparer<DateOnly>.Default, descending)
                .ThenBy(e => e.LastName, text)
                .ThenBy(e => e.FirstName, text),
            _ => ThenBy(By(employees, e => e.LastName, text, descending), e => e.FirstName, text, descending),
        };

        // Ties (for example two people with the same name) keep a stable order.
        return [.. ordered.ThenBy(e => e.Id)];
    }

    private static IOrderedEnumerable<EmployeeDto> By<TKey>(
        IEnumerable<EmployeeDto> employees, Func<EmployeeDto, TKey> key, IComparer<TKey> comparer, bool descending) =>
        descending ? employees.OrderByDescending(key, comparer) : employees.OrderBy(key, comparer);

    private static IOrderedEnumerable<EmployeeDto> ThenBy<TKey>(
        IOrderedEnumerable<EmployeeDto> employees, Func<EmployeeDto, TKey> key, IComparer<TKey> comparer, bool descending) =>
        descending ? employees.ThenByDescending(key, comparer) : employees.ThenBy(key, comparer);
}