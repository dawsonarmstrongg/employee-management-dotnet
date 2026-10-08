using System.Net;
using System.Net.Http.Json;
using EmployeeManagement.Contracts.Employees;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Tests.Integration;

// R-11/R-12: update endpoint — 200 with updated employee, 404, 400, 409 (including the
// "keep your own email" exclusion case).
[Trait("Requirement", "R-11")]
[Trait("Requirement", "R-12")]
public class UpdateEmployeeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    private async Task<EmployeeDto> CreateEmployeeAsync(string? email = null)
    {
        var response = await client.PostAsJsonAsync("/api/employees", TestData.ValidRequest(email));
        return (await response.Content.ReadFromJsonAsync<EmployeeDto>())!;
    }

    [Fact]
    public async Task Update_ExistingEmployee_Returns200WithUpdatedFieldsAndPersistsThem()
    {
        var employee = await CreateEmployeeAsync();
        var request = TestData.ValidRequest(employee.Email);
        request.FirstName = "Updated";
        request.Address!.City = "NewCity";

        var response = await client.PutAsJsonAsync($"/api/employees/{employee.Id}", request);
        var updated = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        var fetched = await client.GetFromJsonAsync<EmployeeDto>($"/api/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Updated", updated!.FirstName);
        Assert.Equal("NewCity", updated.Address.City);
        Assert.Equal("Updated", fetched!.FirstName);
        Assert.Equal("NewCity", fetched.Address.City);
    }

    [Fact]
    public async Task Update_NonExistentId_Returns404()
    {
        var response = await client.PutAsJsonAsync("/api/employees/999999", TestData.ValidRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithInvalidData_Returns400AndLeavesOriginalDataUnchanged()
    {
        var employee = await CreateEmployeeAsync();
        var request = TestData.ValidRequest(employee.Email);
        request.PhoneNumber = "not-a-phone";

        var response = await client.PutAsJsonAsync($"/api/employees/{employee.Id}", request);
        var afterwards = await client.GetFromJsonAsync<EmployeeDto>($"/api/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(employee.PhoneNumber, afterwards!.PhoneNumber);
    }

    [Theory]
    [Trait("Requirement", "R-03")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_WithAnotherEmployeesEmail_Returns409Conflict(bool differentCase)
    {
        var first = await CreateEmployeeAsync();
        var second = await CreateEmployeeAsync();
        var email = differentCase ? first.Email.ToUpperInvariant() : first.Email;
        var request = TestData.ValidRequest(email);

        var response = await client.PutAsJsonAsync($"/api/employees/{second.Id}", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_KeepingOwnEmailUnchanged_Returns200NotConflict(bool differentCase)
    {
        // Exercises the excludeId path in EmployeeService.EmailInUseAsync: an employee's own
        // email must not be treated as "already in use" when they save without changing it.
        var employee = await CreateEmployeeAsync();
        var email = differentCase ? employee.Email.ToUpperInvariant() : employee.Email;
        var request = TestData.ValidRequest(email);

        var response = await client.PutAsJsonAsync($"/api/employees/{employee.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ValidationFailure_ReturnsFieldErrorsMatchingCreate()
    {
        var employee = await CreateEmployeeAsync();
        var request = TestData.ValidRequest(employee.Email);
        request.Address!.Zip = "abc";

        var response = await client.PutAsJsonAsync($"/api/employees/{employee.Id}", request);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.Contains("Address.Zip", problem!.Errors.Keys);
    }
}
