using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Contracts.Validation;

// Runs the DataAnnotations rules on an EmployeeRequest and its nested Address.
// Validator.TryValidateObject does not descend into nested objects, so the address is
// validated separately and its errors are keyed "Address.<Field>".
// Returns field name -> messages, the shape ASP.NET Core uses for a 400 ValidationProblem.
public static class EmployeeRequestValidator
{
    public static Dictionary<string, string[]> Validate(EmployeeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, List<string>>();
        Collect(request, prefix: null, errors);
        if (request.Address is not null)
        {
            Collect(request.Address, nameof(EmployeeRequest.Address), errors);
        }

        return errors.ToDictionary(e => e.Key, e => e.Value.ToArray());
    }

    private static void Collect(object model, string? prefix, Dictionary<string, List<string>> errors)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        foreach (var result in results)
        {
            var members = result.MemberNames.Any() ? result.MemberNames : [string.Empty];
            foreach (var member in members)
            {
                var key = prefix is null ? member : string.IsNullOrEmpty(member) ? prefix : $"{prefix}.{member}";
                if (!errors.TryGetValue(key, out var messages))
                {
                    errors[key] = messages = [];
                }
                messages.Add(result.ErrorMessage ?? "Invalid value.");
            }
        }
    }
}