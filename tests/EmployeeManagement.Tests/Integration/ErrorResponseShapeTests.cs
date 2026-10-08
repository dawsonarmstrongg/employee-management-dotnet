using System.Net;
using System.Net.Http.Json;

namespace EmployeeManagement.Tests.Integration;

// R-14/README: every error under /api is JSON, including framework-produced 404/405,
// and the OpenAPI/Swagger documentation is available.
[Trait("Requirement", "R-14")]
[Trait("Requirement", "R-13")]
public class ErrorResponseShapeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task UnknownApiRoute_Returns404AsJson()
    {
        var response = await client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<html", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnsupportedMethod_OnEmployeesCollection_Returns405AsJson()
    {
        // PATCH isn't mapped on /api/employees (only GET/POST), so this is 405, not 404.
        var response = await client.PatchAsync("/api/employees", content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<html", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MalformedJsonBody_OnCreate_Returns400()
    {
        using var content = new StringContent("{ not valid json", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/employees", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UnknownBrowserRoute_ReturnsHtmlNotFoundPage()
    {
        // Contrast with the /api cases above: a non-API path re-executes to the Blazor not-found page.
        var response = await client.GetAsync("/some-page-that-does-not-exist");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<html", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenApiDocument_IsServedInDevelopment()
    {
        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        Assert.True(json.TryGetProperty("paths", out var paths));
        Assert.True(paths.TryGetProperty("/api/employees", out _));
    }

    [Fact]
    public async Task SwaggerUI_IsServedInDevelopment()
    {
        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
