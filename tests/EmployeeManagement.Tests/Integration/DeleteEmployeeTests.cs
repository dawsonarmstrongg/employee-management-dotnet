using System.Net;
using System.Net.Http.Json;
using EmployeeManagement.Contracts.Employees;
using EmployeeManagement.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagement.Tests.Integration;

// R-11/R-12: delete endpoint — success status, 404, and cascade delete of the address.
[Trait("Requirement", "R-11")]
[Trait("Requirement", "R-12")]
public class DeleteEmployeeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    private async Task<EmployeeDto> CreateEmployeeAsync()
    {
        var response = await client.PostAsJsonAsync("/api/employees", TestData.ValidRequest());
        return (await response.Content.ReadFromJsonAsync<EmployeeDto>())!;
    }

    [Fact]
    public async Task Delete_ExistingEmployee_Returns204NoContent()
    {
        var employee = await CreateEmployeeAsync();

        var response = await client.DeleteAsync($"/api/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingEmployee_EmployeeNoLongerRetrievable()
    {
        var employee = await CreateEmployeeAsync();

        await client.DeleteAsync($"/api/employees/{employee.Id}");
        var getResponse = await client.GetAsync($"/api/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingEmployee_AlsoRemovesTheAddressRowFromTheDatabase()
    {
        var employee = await CreateEmployeeAsync();

        await client.DeleteAsync($"/api/employees/{employee.Id}");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var orphanAddress = await db.Addresses.AnyAsync(a => a.EmployeeId == employee.Id);

        Assert.False(orphanAddress, "Address row should be removed by ON DELETE CASCADE when the employee is deleted.");
    }

    [Fact]
    public async Task Delete_NonExistentId_Returns404()
    {
        var response = await client.DeleteAsync("/api/employees/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SameEmployeeTwice_SecondCallReturns404()
    {
        var employee = await CreateEmployeeAsync();
        await client.DeleteAsync($"/api/employees/{employee.Id}");

        var secondResponse = await client.DeleteAsync($"/api/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_DoesNotRequireAddressInRequestBody()
    {
        // The brief: "Delete does not require an address in the request body." A plain DELETE
        // with no body is exactly what the client above sends; this test documents that intent.
        var employee = await CreateEmployeeAsync();

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/employees/{employee.Id}");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
