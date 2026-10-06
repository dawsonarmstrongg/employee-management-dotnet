namespace EmployeeManagement.Contracts.Employees;

// Cleans up user input before validation and saving: trims every field, uppercases State
// and turns a blank Address2 into null. Shared so the Blazor form validates exactly what
// the API will validate (for example " tx" is accepted and stored as "TX").
public static class EmployeeRequestNormalizer
{
    public static EmployeeRequest Normalize(EmployeeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new EmployeeRequest
        {
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            Email = request.Email?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            DateOfBirth = request.DateOfBirth,
            Address = request.Address is null ? null : new AddressRequest
            {
                Address1 = request.Address.Address1?.Trim(),
                Address2 = string.IsNullOrWhiteSpace(request.Address.Address2) ? null : request.Address.Address2.Trim(),
                City = request.Address.City?.Trim(),
                State = request.Address.State?.Trim().ToUpperInvariant(),
                Zip = request.Address.Zip?.Trim(),
            },
        };
    }
}