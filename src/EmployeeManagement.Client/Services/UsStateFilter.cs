using EmployeeManagement.Contracts.Validation;

namespace EmployeeManagement.Client.Services;

public static class UsStateFilter
{
    // Code prefix matches first ("N" -> NE, NV...), then name prefix matches ("TE" -> Tennessee, Texas).
    public static IReadOnlyList<UsState> Filter(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return UsStates.All;
        }

        var term = text.Trim();
        var byCode = UsStates.All.Where(s => s.Code.StartsWith(term, StringComparison.OrdinalIgnoreCase));
        var byName = UsStates.All.Where(s => s.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase));
        return byCode.Concat(byName).Distinct().ToList();
    }

    // Keeps letters only, max 2, uppercased: "t" -> "T", " tx " -> "TX", "Texas" -> "TE".
    public static string Clean(string? value) =>
        new string((value ?? string.Empty).Where(char.IsAsciiLetter).Take(2).ToArray()).ToUpperInvariant();
}