using EmployeeManagement.Contracts.Employees;
using EmployeeManagement.Contracts.Validation;

namespace EmployeeManagement.Tests.Unit;

// R-02..R-09, R-28: required fields, email format, phone format, address required fields,
// state code, zip format, and the documented DateOfBirth-instead-of-age deviation.
[Trait("Requirement", "R-02")]
[Trait("Requirement", "R-03")]
[Trait("Requirement", "R-04")]
[Trait("Requirement", "R-05")]
[Trait("Requirement", "R-06")]
[Trait("Requirement", "R-07")]
[Trait("Requirement", "R-08")]
[Trait("Requirement", "R-09")]
[Trait("Requirement", "R-28")]
public class EmployeeRequestValidatorTests
{
    private static EmployeeRequest Valid() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        PhoneNumber = "(555)-123-4567",
        DateOfBirth = new DateOnly(1990, 1, 1),
        Address = new AddressRequest
        {
            Address1 = "123 Main St",
            City = "Austin",
            State = "TX",
            Zip = "73301",
        },
    };

    [Fact]
    public void Validate_CompletelyValidRequest_ReturnsNoErrors()
    {
        var errors = EmployeeRequestValidator.Validate(Valid());

        Assert.Empty(errors);
    }

    // FirstName/LastName only need [Required]; Email also needs a format check ("not-an-email").
    // One Theory, keyed by property name via reflection, covers all three the same way CreateEmployeeTests does at the HTTP layer.
    [Theory]
    [InlineData(nameof(EmployeeRequest.FirstName), null)]
    [InlineData(nameof(EmployeeRequest.FirstName), "")]
    [InlineData(nameof(EmployeeRequest.FirstName), "   ")]
    [InlineData(nameof(EmployeeRequest.LastName), null)]
    [InlineData(nameof(EmployeeRequest.LastName), "")]
    [InlineData(nameof(EmployeeRequest.LastName), "   ")]
    [InlineData(nameof(EmployeeRequest.Email), null)]
    [InlineData(nameof(EmployeeRequest.Email), "")]
    [InlineData(nameof(EmployeeRequest.Email), "not-an-email")]
    public void Validate_MissingOrInvalidTopLevelField_ReturnsFieldError(string field, string? value)
    {
        var request = Valid();
        typeof(EmployeeRequest).GetProperty(field)!.SetValue(request, value);

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains(field, errors.Keys);
    }

    [Theory]
    // R-04: must match (XXX)-XXX-XXXX.
    [InlineData(null)]
    [InlineData("")]
    [InlineData("555-123-4567")]
    [InlineData("(555)123-4567")]
    [InlineData("(555)-1234-567")]
    [InlineData("(55)-123-4567")]
    [InlineData("(555)-123-456")]
    [InlineData("(555) 123-4567")]
    [InlineData("5551234567")]
    [InlineData("(555)-abc-4567")]
    public void Validate_InvalidPhoneFormat_ReturnsPhoneNumberError(string? phone)
    {
        var request = Valid();
        request.PhoneNumber = phone;

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains(nameof(EmployeeRequest.PhoneNumber), errors.Keys);
    }

    [Fact]
    public void Validate_ExactlyCorrectPhoneFormat_Passes()
    {
        var request = Valid();
        request.PhoneNumber = "(555)-123-4567";

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.DoesNotContain(nameof(EmployeeRequest.PhoneNumber), errors.Keys);
    }

    [Fact]
    public void Validate_MissingDateOfBirth_ReturnsDateOfBirthError()
    {
        var request = Valid();
        request.DateOfBirth = null;

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains(nameof(EmployeeRequest.DateOfBirth), errors.Keys);
    }

    [Fact]
    public void Validate_FutureDateOfBirth_ReturnsError()
    {
        var request = Valid();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains(nameof(EmployeeRequest.DateOfBirth), errors.Keys);
    }

    [Fact]
    public void Validate_DateOfBirthOfToday_IsValid()
    {
        var request = Valid();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.Today);

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.DoesNotContain(nameof(EmployeeRequest.DateOfBirth), errors.Keys);
    }

    [Fact]
    public void Validate_DateOfBirthBeforeEarliestAllowed_ReturnsError()
    {
        var request = Valid();
        request.DateOfBirth = new DateOnly(1899, 12, 31);

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains(nameof(EmployeeRequest.DateOfBirth), errors.Keys);
    }

    [Fact]
    public void Validate_DateOfBirthExactlyEarliestAllowed_IsValid()
    {
        var request = Valid();
        request.DateOfBirth = new DateOnly(1900, 1, 1);

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.DoesNotContain(nameof(EmployeeRequest.DateOfBirth), errors.Keys);
    }

    // Address1/City only need [Required]; State and Zip have their own format-boundary Theories below.
    [Theory]
    [InlineData(nameof(AddressRequest.Address1), null)]
    [InlineData(nameof(AddressRequest.Address1), "")]
    [InlineData(nameof(AddressRequest.Address1), "   ")]
    [InlineData(nameof(AddressRequest.City), null)]
    [InlineData(nameof(AddressRequest.City), "")]
    public void Validate_MissingRequiredAddressField_ReturnsNestedAddressError(string field, string? value)
    {
        var request = Valid();
        typeof(AddressRequest).GetProperty(field)!.SetValue(request.Address, value);

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains($"Address.{field}", errors.Keys);
    }

    [Fact]
    public void Validate_MissingAddress2_IsValid_BecauseOptional()
    {
        var request = Valid();
        request.Address!.Address2 = null;

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.DoesNotContain("Address.Address2", errors.Keys);
    }

    [Theory]
    // R-08: must be a 2-letter US state code.
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ZZ")]
    [InlineData("Texas")]
    [InlineData("T")]
    [InlineData("TXX")]
    public void Validate_InvalidState_ReturnsNestedAddressError(string? state)
    {
        var request = Valid();
        request.Address!.State = state;

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains("Address.State", errors.Keys);
    }

    [Theory]
    [InlineData("TX")]
    [InlineData("DC")]
    [InlineData("tx")]
    public void Validate_ValidState_AnyCase_Passes(string state)
    {
        var request = Valid();
        request.Address!.State = state;

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.DoesNotContain("Address.State", errors.Keys);
    }

    [Theory]
    // R-09: 5-digit US ZIP code.
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("1234A")]
    [InlineData("12345-6789")]
    public void Validate_InvalidZip_ReturnsNestedAddressError(string? zip)
    {
        var request = Valid();
        request.Address!.Zip = zip;

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.Contains("Address.Zip", errors.Keys);
    }

    [Fact]
    public void Validate_FiveDigitZip_Passes()
    {
        var request = Valid();
        request.Address!.Zip = "73301";

        var errors = EmployeeRequestValidator.Validate(request);

        Assert.DoesNotContain("Address.Zip", errors.Keys);
    }

    [Fact]
    public void Validate_EveryFieldMissing_ReturnsOneErrorKeyPerField()
    {
        var request = new EmployeeRequest { Address = new AddressRequest() };

        var errors = EmployeeRequestValidator.Validate(request);

        // 5 top-level fields (FirstName, LastName, Email, PhoneNumber, DateOfBirth)
        // + 4 required nested address fields (Address1, City, State, Zip; Address2 is optional).
        Assert.Contains(nameof(EmployeeRequest.FirstName), errors.Keys);
        Assert.Contains(nameof(EmployeeRequest.LastName), errors.Keys);
        Assert.Contains(nameof(EmployeeRequest.Email), errors.Keys);
        Assert.Contains(nameof(EmployeeRequest.PhoneNumber), errors.Keys);
        Assert.Contains(nameof(EmployeeRequest.DateOfBirth), errors.Keys);
        Assert.Contains("Address.Address1", errors.Keys);
        Assert.Contains("Address.City", errors.Keys);
        Assert.Contains("Address.State", errors.Keys);
        Assert.Contains("Address.Zip", errors.Keys);
    }

    [Fact]
    public void Validate_NullRequest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => EmployeeRequestValidator.Validate(null!));
    }
}
