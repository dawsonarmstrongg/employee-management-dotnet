using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Client.Services;

// Typed wrapper around the /api/employees endpoints. Turns HTTP responses into results the UI can show.
public sealed class EmployeeApiClient(HttpClient http)
{
    private const string BasePath = "api/employees";

    /// <exception cref="HttpRequestException">The server could not be reached or returned an error.</exception>
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<List<EmployeeDto>>(BasePath, cancellationToken) ?? [];

    /// <exception cref="HttpRequestException">The server could not be reached.</exception>
    public async Task<SaveEmployeeResult> CreateAsync(EmployeeRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(BasePath, request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<EmployeeDto>(cancellationToken);
            return SaveEmployeeResult.Saved(created!);
        }

        var problem = await ReadProblemAsync(response, cancellationToken);
        return response.StatusCode switch
        {
            // 400 with per-field errors, keyed like "Email" or "Address.Zip".
            HttpStatusCode.BadRequest when problem?.Errors is { Count: > 0 } => SaveEmployeeResult.Invalid(problem.Errors),

            // 409 is about the email, so show it next to the Email field.
            HttpStatusCode.Conflict => SaveEmployeeResult.Invalid(new Dictionary<string, string[]>
            {
                [nameof(EmployeeRequest.Email)] = [problem?.Detail ?? "That email address is already in use."],
            }),

            _ => SaveEmployeeResult.Failed(
                problem?.Detail ?? problem?.Title ?? $"The server returned {(int)response.StatusCode} {response.ReasonPhrase}."),
        };
    }

    private static async Task<ApiProblem?> ReadProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiProblem>(cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return null; // Body was empty or not JSON; fall back to the status code.
        }
    }

    // The fields of an RFC 9457 problem details response that the UI uses.
    private sealed record ApiProblem(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}

// Outcome of a save: exactly one of Employee, Errors or ErrorMessage is set.
public sealed record SaveEmployeeResult(
    EmployeeDto? Employee,
    IReadOnlyDictionary<string, string[]>? Errors,
    string? ErrorMessage)
{
    public static SaveEmployeeResult Saved(EmployeeDto employee) => new(employee, null, null);
    public static SaveEmployeeResult Invalid(IReadOnlyDictionary<string, string[]> errors) => new(null, errors, null);
    public static SaveEmployeeResult Failed(string message) => new(null, null, message);
}