using EmployeeManagement.Client.Services;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Tests.Unit;

// R-15: table shows Name, Email, Phone, Address (Address1/2/City/State/Zip combined).
[Trait("Requirement", "R-15")]
public class EmployeeDisplayTests
{
    private static EmployeeDto MakeEmployee(string first = "Jane", string last = "Doe") => new(
        1, first, last, "jane.doe@example.com", "(555)-123-4567", new DateOnly(1990, 1, 1),
        new AddressDto("123 Main St", "Apt 4B", "Springfield", "IL", "62701"));

    [Fact]
    public void FullName_CombinesFirstAndLastNameWithASpace()
    {
        Assert.Equal("Jane Doe", EmployeeDisplay.FullName(MakeEmployee()));
    }

    [Fact]
    public void SortableName_ShowsLastNameFirst()
    {
        Assert.Equal("Doe, Jane", EmployeeDisplay.SortableName(MakeEmployee()));
    }

    [Fact]
    public void FullAddress_WithAddress2_CombinesAllFiveParts()
    {
        var address = new AddressDto("123 Main St", "Apt 4B", "Springfield", "IL", "62701");

        var result = EmployeeDisplay.FullAddress(address);

        Assert.Equal("123 Main St, Apt 4B, Springfield, IL 62701", result);
    }

    [Fact]
    public void FullAddress_WithoutAddress2_SkipsItWithNoBlankSegment()
    {
        var address = new AddressDto("456 Oak Ave", null, "Austin", "TX", "73301");

        var result = EmployeeDisplay.FullAddress(address);

        Assert.Equal("456 Oak Ave, Austin, TX 73301", result);
        Assert.DoesNotContain(",,", result);
    }

    [Fact]
    public void FullAddress_WithBlankAddress2_SkipsItWithNoBlankSegment()
    {
        var address = new AddressDto("456 Oak Ave", "   ", "Austin", "TX", "73301");

        var result = EmployeeDisplay.FullAddress(address);

        Assert.Equal("456 Oak Ave, Austin, TX 73301", result);
    }

    [Fact]
    public void DateOfBirth_FormatsAsMonthDayYear()
    {
        var result = EmployeeDisplay.DateOfBirth(new DateOnly(1991, 3, 14));

        Assert.Equal("03/14/1991", result);
    }
}
