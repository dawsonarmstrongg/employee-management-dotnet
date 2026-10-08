using System.Net;
using System.Net.Http.Json;
using EmployeeManagement.Contracts.Employees;

namespace EmployeeManagement.Tests.Integration;

// R-01/R-11/R-12 (plus R-10 seed count, R-14 problem details): seeded data, GET all, GET by id.
[Trait("Requirement", "R-01")]
[Trait("Requirement", "R-11")]
[Trait("Requirement", "R-12")]
public class GetEmployeesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    [Trait("Requirement", "R-10")]
    public async Task GetAll_FreshDatabase_SeedsBetweenThreeAndFiveEmployees()
    {
        var response = await client.GetAsync("/api/employees");
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(employees);
        Assert.InRange(employees!.Count, 3, 5);
    }

    [Fact]
    public async Task GetAll_EverySeededEmployee_HasANestedAddress()
    {
        var employees = await client.GetFromJsonAsync<List<EmployeeDto>>("/api/employees");

        Assert.All(employees!, e =>
        {
            Assert.NotNull(e.Address);
            Assert.False(string.IsNullOrWhiteSpace(e.Address.Address1));
            Assert.False(string.IsNullOrWhiteSpace(e.Address.City));
            Assert.False(string.IsNullOrWhiteSpace(e.Address.State));
            Assert.False(string.IsNullOrWhiteSpace(e.Address.Zip));
        });
    }

    [Fact]
    public async Task GetAll_ReturnsEmployeesSortedByLastNameThenFirstName()
    {
        var employees = await client.GetFromJsonAsync<List<EmployeeDto>>("/api/employees");

        var expectedOrder = employees!
            .OrderBy(e => e.LastName, StringComparer.Ordinal)
            .ThenBy(e => e.FirstName, StringComparer.Ordinal)
            .Select(e => e.Id);

        Assert.Equal(expectedOrder, employees!.Select(e => e.Id));
    }

    [Fact]
    public async Task GetById_ExistingSeededEmployee_ReturnsEmployeeWithAddress()
    {
        var all = await client.GetFromJsonAsync<List<EmployeeDto>>("/api/employees");
        var firstId = all![0].Id;

        var response = await client.GetAsync($"/api/employees/{firstId}");
        var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(firstId, employee!.Id);
        Assert.NotNull(employee.Address);
    }

    [Fact]
    [Trait("Requirement", "R-14")]
    public async Task GetById_NonExistentId_Returns404WithProblemDetails()
    {
        var response = await client.GetAsync("/api/employees/999999");
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(404, problem!.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));

        // QA-01 (fixed): problem details use the application/problem+json media type (RFC 9457).
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetById_NonIntegerId_Returns404AsJsonNotHtml()
    {
        // README documents this explicitly: /api/employees/abc has no matching route ({id:int}),
        // and every error under /api must still be JSON, not the Blazor HTML "not found" page.
        var response = await client.GetAsync("/api/employees/abc");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<html", body, StringComparison.OrdinalIgnoreCase);
    }
}
