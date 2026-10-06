namespace EmployeeManagement.Client.Services;

// Formats a phone number as the user types: "5551234567" -> "(555)-123-4567".
public static class PhoneNumberFormatter
{
    private const int DigitCount = 10;

    // Keeps only the digits (at most 10) and adds the separators that belong *before* each typed digit.
    // It never adds a trailing separator, so backspace always deletes a digit:
    //   "5" -> "(5", "5551" -> "(555)-1", "5551234" -> "(555)-123-4".
    // A pasted 11-digit number starting with the US country code 1 drops the 1.
    public static string FormatAsYouType(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var digits = new string(input.Where(char.IsAsciiDigit).ToArray());
        if (digits.Length == DigitCount + 1 && digits[0] == '1')
        {
            digits = digits[1..];
        }
        if (digits.Length > DigitCount)
        {
            digits = digits[..DigitCount];
        }

        return digits.Length switch
        {
            0 => string.Empty,
            <= 3 => $"({digits}",
            <= 6 => $"({digits[..3]})-{digits[3..]}",
            _ => $"({digits[..3]})-{digits[3..6]}-{digits[6..]}",
        };
    }
}