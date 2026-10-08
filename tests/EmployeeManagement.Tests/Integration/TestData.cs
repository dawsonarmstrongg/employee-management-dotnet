using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Tests.Integration;

// Shared helpers for building valid request bodies with unique emails, so tests
// never collide with seed data or with each other.
internal static class TestData
{
    public static EmployeeRequest ValidRequest(string? email = null) => new()
    {
        FirstName = "Test",
        LastName = "User",
        Email = email ?? UniqueEmail(),
        PhoneNumber = "(555)-999-0000",
        DateOfBirth = new DateOnly(1992, 6, 15),
        Address = new AddressRequest
        {
            Address1 = "100 Test Way",
            City = "Testville",
            State = "TX",
            Zip = "73301",
        },
    };

    public static string UniqueEmail() => $"qa-{Guid.NewGuid():N}@example.com";
}
