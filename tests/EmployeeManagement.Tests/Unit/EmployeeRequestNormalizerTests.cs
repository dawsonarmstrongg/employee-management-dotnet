using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Tests.Unit;

// R-10: input is normalized before validation (trim, uppercase state, blank Address2 -> null).
[Trait("Requirement", "R-10")]
public class EmployeeRequestNormalizerTests
{
    private static EmployeeRequest Build(
        string? firstName = "  John  ",
        string? lastName = "  Doe  ",
        string? email = "  John.Doe@Example.com  ",
        string? phone = "  (555)-123-4567  ",
        string? address2 = "   ",
        string? state = " tx ") => new()
    {
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        PhoneNumber = phone,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Address = new AddressRequest
        {
            Address1 = "  123 Main St  ",
            Address2 = address2,
            City = "  Austin  ",
            State = state,
            Zip = "  73301  ",
        },
    };

    [Fact]
    public void Normalize_TrimsLeadingAndTrailingWhitespaceFromTextFields()
    {
        var result = EmployeeRequestNormalizer.Normalize(Build());

        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("John.Doe@Example.com", result.Email);
        Assert.Equal("(555)-123-4567", result.PhoneNumber);
        Assert.Equal("123 Main St", result.Address!.Address1);
        Assert.Equal("Austin", result.Address.City);
        Assert.Equal("73301", result.Address.Zip);
    }

    [Theory]
    [InlineData(" tx ", "TX")]
    [InlineData("tx", "TX")]
    [InlineData("TX", "TX")]
    [InlineData("Tx", "TX")]
    public void Normalize_UppercasesState(string input, string expected)
    {
        var result = EmployeeRequestNormalizer.Normalize(Build(state: input));

        Assert.Equal(expected, result.Address!.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_BlankAddress2_BecomesNull(string? address2)
    {
        var result = EmployeeRequestNormalizer.Normalize(Build(address2: address2));

        Assert.Null(result.Address!.Address2);
    }

    [Fact]
    public void Normalize_NonBlankAddress2_IsTrimmedButKept()
    {
        var result = EmployeeRequestNormalizer.Normalize(Build(address2: "  Apt 4B  "));

        Assert.Equal("Apt 4B", result.Address!.Address2);
    }

    [Fact]
    public void Normalize_NullFields_RemainNull()
    {
        var request = new EmployeeRequest { Address = new AddressRequest() };

        var result = EmployeeRequestNormalizer.Normalize(request);

        Assert.Null(result.FirstName);
        Assert.Null(result.LastName);
        Assert.Null(result.Email);
        Assert.Null(result.PhoneNumber);
        Assert.Null(result.Address!.Address1);
        Assert.Null(result.Address.State);
    }

    [Fact]
    public void Normalize_NullAddress_RemainsNull()
    {
        var request = new EmployeeRequest { Address = null };

        var result = EmployeeRequestNormalizer.Normalize(request);

        Assert.Null(result.Address);
    }

    [Fact]
    public void Normalize_NullRequest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => EmployeeRequestNormalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_DoesNotMutateOriginalRequest()
    {
        var original = Build();

        EmployeeRequestNormalizer.Normalize(original);

        // The normalizer must return a new object, not mutate the one passed in,
        // otherwise re-validating the same instance twice would see already-trimmed values.
        Assert.Equal("  John  ", original.FirstName);
    }
}
