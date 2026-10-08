using EmployeeManagement.Client.Services;

namespace EmployeeManagement.Tests.Unit;

// R-17: phone number formats as the user types (Client/Services/PhoneNumberFormatter.cs).
[Trait("Requirement", "R-17")]
public class PhoneNumberFormatterTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("5", "(5")]
    [InlineData("55", "(55")]
    [InlineData("555", "(555")]
    [InlineData("5551", "(555)-1")]
    [InlineData("555123", "(555)-123")]
    [InlineData("5551234", "(555)-123-4")]
    [InlineData("5551234567", "(555)-123-4567")]
    public void FormatAsYouType_BuildsSeparatorsProgressively(string? input, string expected)
    {
        var result = PhoneNumberFormatter.FormatAsYouType(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FormatAsYouType_StripsNonDigitCharacters()
    {
        var result = PhoneNumberFormatter.FormatAsYouType("abc-555-def-123-4567");

        Assert.Equal("(555)-123-4567", result);
    }

    [Fact]
    public void FormatAsYouType_MoreThanTenDigits_TruncatesToTen()
    {
        var result = PhoneNumberFormatter.FormatAsYouType("55512345678999");

        Assert.Equal("(555)-123-4567", result);
    }

    [Fact]
    public void FormatAsYouType_ElevenDigitsStartingWithCountryCodeOne_DropsLeadingOne()
    {
        var result = PhoneNumberFormatter.FormatAsYouType("15551234567");

        Assert.Equal("(555)-123-4567", result);
    }

    [Fact]
    public void FormatAsYouType_ElevenDigitsNotStartingWithOne_KeepsFirstTenDigits()
    {
        // No country-code stripping rule applies when the leading digit isn't 1.
        var result = PhoneNumberFormatter.FormatAsYouType("25551234567");

        Assert.Equal("(255)-512-3456", result);
    }

    [Fact]
    public void FormatAsYouType_PastedFormattedNumberWithCountryCode_ReformatsCleanly()
    {
        var result = PhoneNumberFormatter.FormatAsYouType("+1 (555) 222-3333");

        Assert.Equal("(555)-222-3333", result);
    }

    [Fact]
    public void FormatAsYouType_NeverAddsATrailingSeparator()
    {
        // So backspace always removes a digit, never just a separator.
        var result = PhoneNumberFormatter.FormatAsYouType("555");

        Assert.False(result.EndsWith('-'));
        Assert.False(result.EndsWith(')'));
    }
}
