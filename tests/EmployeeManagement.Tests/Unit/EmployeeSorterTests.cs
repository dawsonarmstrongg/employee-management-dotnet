using EmployeeManagement.Client.Services;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Tests.Unit;

// R-16: table supports sorting (click to sort/reverse); default order matches the API's (last, first).
[Trait("Requirement", "R-16")]
public class EmployeeSorterTests
{
    private static EmployeeDto Make(int id, string first, string last, string email, int birthYear) => new(
        id, first, last, email, "(555)-123-4567", new DateOnly(birthYear, 1, 1),
        new AddressDto("1 Main St", null, "Austin", "TX", "73301"));

    private static readonly List<EmployeeDto> Sample =
    [
        Make(1, "John", "Smith", "john.smith@example.com", 1980),
        Make(2, "Jane", "Doe", "jane.doe@example.com", 1991),
        Make(3, "Amy", "Doe", "amy.doe@example.com", 1975),
    ];

    [Fact]
    public void Sort_DefaultSort_OrdersByLastNameThenFirstNameAscending()
    {
        var result = EmployeeSorter.Sort(Sample, EmployeeSort.Default);

        Assert.Equal(["Amy", "Jane", "John"], result.Select(e => e.FirstName));
        Assert.Equal(["Doe", "Doe", "Smith"], result.Select(e => e.LastName));
    }

    [Fact]
    public void Sort_NameDescending_ReversesOrder()
    {
        var sort = new EmployeeSort(EmployeeSortColumn.Name, SortDirection.Descending);

        var result = EmployeeSorter.Sort(Sample, sort);

        Assert.Equal(["Smith", "Doe", "Doe"], result.Select(e => e.LastName));
    }

    [Fact]
    public void Sort_ByEmail_OrdersAlphabetically()
    {
        var sort = new EmployeeSort(EmployeeSortColumn.Email, SortDirection.Ascending);

        var result = EmployeeSorter.Sort(Sample, sort);

        Assert.Equal(
            ["amy.doe@example.com", "jane.doe@example.com", "john.smith@example.com"],
            result.Select(e => e.Email));
    }

    [Fact]
    public void Toggle_SameColumnTwice_FlipsDirection()
    {
        var sort = EmployeeSort.Default.Toggle(EmployeeSortColumn.Name);

        Assert.Equal(EmployeeSortColumn.Name, sort.Column);
        Assert.Equal(SortDirection.Descending, sort.Direction);
    }

    [Fact]
    public void Toggle_DifferentColumn_ResetsToAscending()
    {
        var descendingByName = new EmployeeSort(EmployeeSortColumn.Name, SortDirection.Descending);

        var sort = descendingByName.Toggle(EmployeeSortColumn.Email);

        Assert.Equal(EmployeeSortColumn.Email, sort.Column);
        Assert.Equal(SortDirection.Ascending, sort.Direction);
    }

    [Fact]
    public void Sort_TiesOnSortKey_BreaksTieByIdForAStableOrder()
    {
        var tied = new List<EmployeeDto>
        {
            Make(5, "Sam", "Lee", "sam5@example.com", 1990),
            Make(2, "Sam", "Lee", "sam2@example.com", 1990),
        };

        var result = EmployeeSorter.Sort(tied, EmployeeSort.Default);

        Assert.Equal([2, 5], result.Select(e => e.Id));
    }

    [Fact]
    public void Sort_EmptyList_ReturnsEmptyList()
    {
        var result = EmployeeSorter.Sort([], EmployeeSort.Default);

        Assert.Empty(result);
    }
}
