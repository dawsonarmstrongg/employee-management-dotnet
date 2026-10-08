using EmployeeManagement.Client.Services;

namespace EmployeeManagement.Tests.Unit;

// R-08/R-18: state picker filters by code then name and only accepts 2-letter codes.
[Trait("Requirement", "R-08")]
[Trait("Requirement", "R-18")]
public class UsStateFilterTests
{
    [Fact]
    public void Filter_EmptyText_ReturnsAllStates()
    {
        var result = UsStateFilter.Filter(null);

        Assert.Equal(51, result.Count); // 50 states + DC
    }

    [Fact]
    public void Filter_SingleLetter_MatchesByCodePrefixFirst()
    {
        var result = UsStateFilter.Filter("T");

        Assert.Contains(result, s => s.Code == "TN");
        Assert.Contains(result, s => s.Code == "TX");
        // Code matches come before name matches, so TN/TX (by code) appear before Tennessee/Texas (by name - same ones here).
        Assert.Equal("TN", result[0].Code);
    }

    [Fact]
    public void Filter_TwoLetters_MatchesByNamePrefixWhenNoCodeMatches()
    {
        var result = UsStateFilter.Filter("TE");

        Assert.Contains(result, s => s.Name == "Tennessee");
        Assert.Contains(result, s => s.Name == "Texas");
    }

    [Fact]
    public void Filter_UnmatchedText_ReturnsEmpty()
    {
        var result = UsStateFilter.Filter("ZZ");

        Assert.Empty(result);
    }

    [Fact]
    public void Filter_IsCaseInsensitive()
    {
        var lower = UsStateFilter.Filter("tx");
        var upper = UsStateFilter.Filter("TX");

        Assert.Equal(upper.Select(s => s.Code), lower.Select(s => s.Code));
    }

    [Theory]
    [InlineData("t", "T")]
    [InlineData(" tx ", "TX")]
    [InlineData("Texas", "TE")]
    [InlineData("t1x2", "TX")]
    [InlineData("", "")]
    public void Clean_StripsNonLettersUppercasesAndLimitsToTwoChars(string input, string expected)
    {
        var result = UsStateFilter.Clean(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Clean_NullValue_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, UsStateFilter.Clean(null));
    }
}
