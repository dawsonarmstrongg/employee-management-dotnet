using Bunit;
using EmployeeManagement.Client.Components;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Tests.Components;

// R-15: the employee table must show exactly Name, Email, Phone, Address (combined),
// plus the documented extra Date of birth column (see AI_USAGE/README deviation).
[Trait("Requirement", "R-15")]
public class EmployeeTableTests : BunitContext
{
    private static EmployeeDto Employee(int id = 1) => new(
        id, "Jane", "Doe", "jane.doe@example.com", "(555)-123-4567", new DateOnly(1991, 3, 14),
        new AddressDto("123 Main St", "Apt 4B", "Springfield", "IL", "62701"));

    [Fact]
    public void Render_ShowsNameAsLastCommaFirst()
    {
        var cut = Render<EmployeeTable>(p => p.Add(c => c.Employees, [Employee()]));

        Assert.Contains("Doe, Jane", cut.Markup);
    }

    [Fact]
    public void Render_ShowsEmailAsMailtoLink()
    {
        var cut = Render<EmployeeTable>(p => p.Add(c => c.Employees, [Employee()]));

        var link = cut.Find("a[href='mailto:jane.doe@example.com']");
        Assert.Equal("jane.doe@example.com", link.TextContent);
    }

    [Fact]
    public void Render_ShowsPhoneNumberVerbatim()
    {
        var cut = Render<EmployeeTable>(p => p.Add(c => c.Employees, [Employee()]));

        Assert.Contains("(555)-123-4567", cut.Markup);
    }

    [Fact]
    public void Render_CombinesAddressPartsIntoOneCell()
    {
        var cut = Render<EmployeeTable>(p => p.Add(c => c.Employees, [Employee()]));

        Assert.Contains("123 Main St, Apt 4B, Springfield, IL 62701", cut.Markup);
    }

    [Fact]
    public void Render_NewlyAddedEmployee_ShowsNewBadge()
    {
        var employee = Employee();
        var cut = Render<EmployeeTable>(p => p
            .Add(c => c.Employees, [employee])
            .Add(c => c.NewIds, new HashSet<int> { employee.Id }));

        Assert.Contains("New", cut.Markup);
    }

    [Fact]
    [Trait("Requirement", "R-16")]
    public void Render_ClickingNameHeader_TogglesSortOrder()
    {
        var a = Employee(1) with { FirstName = "Amy", LastName = "Zed", Email = "amy@example.com" };
        var z = Employee(2) with { FirstName = "Zoe", LastName = "Abbot", Email = "zoe@example.com" };
        var cut = Render<EmployeeTable>(p => p.Add(c => c.Employees, [a, z]));

        // Default sort is by last name ascending: Abbot (Zoe) before Zed (Amy).
        var rowsBefore = cut.FindAll("tbody tr");
        Assert.Contains("Zoe", rowsBefore[0].TextContent);

        cut.Find("th button").Click(); // Name header -> toggles to descending

        var rowsAfter = cut.FindAll("tbody tr");
        Assert.Contains("Amy", rowsAfter[0].TextContent);
    }
}
