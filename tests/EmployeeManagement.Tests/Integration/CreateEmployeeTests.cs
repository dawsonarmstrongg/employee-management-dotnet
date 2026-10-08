using System.Net;
using System.Net.Http.Json;
using EmployeeManagement.Contracts.Employees;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Tests.Integration;

// R-02..R-12: create endpoint — validation, duplicate email, normalization, status codes.
[Trait("Requirement", "R-11")]
[Trait("Requirement", "R-12")]
public class CreateEmployeeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    [Trait("Requirement", "R-11")]
    public async Task Create_WithValidData_Returns201WithLocationHeaderAndNestedAddress()
    {
        var request = TestData.ValidRequest();

        var response = await client.PostAsJsonAsync("/api/employees", request);
        var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.NotNull(created!.Address);
        Assert.Equal(request.Address!.Address1, created.Address.Address1);
        Assert.Equal(request.Address.City, created.Address.City);
    }

    [Fact]
    public async Task Create_WithValidData_IsRetrievableAfterwardsWithSameData()
    {
        var email = TestData.UniqueEmail();
        var request = TestData.ValidRequest(email);

        var createResponse = await client.PostAsJsonAsync("/api/employees", request);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        var getResponse = await client.GetAsync($"/api/employees/{created!.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<EmployeeDto>();

        Assert.Equal(email, fetched!.Email);
        Assert.Equal(request.PhoneNumber, fetched.PhoneNumber);
        Assert.Equal(request.Address!.Zip, fetched.Address.Zip);
    }

    [Theory]
    [Trait("Requirement", "R-02")]
    [InlineData(nameof(EmployeeRequest.FirstName))]
    [InlineData(nameof(EmployeeRequest.LastName))]
    [InlineData(nameof(EmployeeRequest.Email))]
    [InlineData(nameof(EmployeeRequest.PhoneNumber))]
    public async Task Create_WithMissingRequiredField_Returns400WithFieldError(string missingField)
    {
        var request = TestData.ValidRequest();
        typeof(EmployeeRequest).GetProperty(missingField)!.SetValue(request, null);

        var response = await client.PostAsJsonAsync("/api/employees", request);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(missingField, problem!.Errors.Keys);
    }

    // Phone, State and Zip each have their own exhaustive boundary matrix in EmployeeRequestValidatorTests (Unit);
    // this Theory only confirms the API wires an invalid value in any of the three shapes to the matching 400 field key.
    [Theory]
    [Trait("Requirement", "R-04")]
    [Trait("Requirement", "R-08")]
    [Trait("Requirement", "R-09")]
    [InlineData(nameof(EmployeeRequest.PhoneNumber), "555-123-4567")]
    [InlineData(nameof(EmployeeRequest.PhoneNumber), "5551234567")]
    [InlineData("Address.State", "ZZ")]
    [InlineData("Address.State", "Texas")]
    [InlineData("Address.Zip", "1234")]
    [InlineData("Address.Zip", "123456")]
    public async Task Create_WithInvalidFieldValue_Returns400WithMatchingFieldError(string fieldKey, string invalidValue)
    {
        var request = TestData.ValidRequest();
        if (fieldKey.StartsWith("Address.", StringComparison.Ordinal))
        {
            typeof(AddressRequest).GetProperty(fieldKey["Address.".Length..])!.SetValue(request.Address, invalidValue);
        }
        else
        {
            typeof(EmployeeRequest).GetProperty(fieldKey)!.SetValue(request, invalidValue);
        }

        var response = await client.PostAsJsonAsync("/api/employees", request);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(fieldKey, problem!.Errors.Keys);
    }

    [Theory]
    [Trait("Requirement", "R-03")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_WithDuplicateEmail_Returns409Conflict(bool differentCase)
    {
        var email = TestData.UniqueEmail();
        await client.PostAsJsonAsync("/api/employees", TestData.ValidRequest(email));
        var secondEmail = differentCase ? email.ToUpperInvariant() : email;

        var response = await client.PostAsJsonAsync("/api/employees", TestData.ValidRequest(secondEmail));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Requirement", "R-10")]
    public async Task Create_WithLowercaseStateAndPaddedFields_NormalizesBeforeStoring()
    {
        var request = TestData.ValidRequest();
        request.FirstName = "  Pat  ";
        request.Address!.State = " tx ";
        request.Address.Address2 = "   ";

        var response = await client.PostAsJsonAsync("/api/employees", request);
        var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Pat", created!.FirstName);
        Assert.Equal("TX", created.Address.State);
        Assert.Null(created.Address.Address2);
    }

    [Fact]
    [Trait("Requirement", "R-05")]
    [Trait("Requirement", "R-07")]
    [Trait("Requirement", "R-08")]
    [Trait("Requirement", "R-09")]
    public async Task Create_WithMissingAddressFields_Returns400WithAddressErrors()
    {
        var request = TestData.ValidRequest();
        request.Address = new AddressRequest();

        var response = await client.PostAsJsonAsync("/api/employees", request);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Address.Address1", problem!.Errors.Keys);
        Assert.Contains("Address.City", problem.Errors.Keys);
        Assert.Contains("Address.State", problem.Errors.Keys);
        Assert.Contains("Address.Zip", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_RejectedByValidation_DoesNotPersistAnything()
    {
        var before = await client.GetFromJsonAsync<List<EmployeeDto>>("/api/employees");
        var request = TestData.ValidRequest();
        request.Email = "not-an-email";

        await client.PostAsJsonAsync("/api/employees", request);
        var after = await client.GetFromJsonAsync<List<EmployeeDto>>("/api/employees");

        Assert.Equal(before!.Count, after!.Count);
    }
}
