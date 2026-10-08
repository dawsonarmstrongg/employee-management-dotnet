using EmployeeManagement.Server;
using EmployeeManagement.Server.Components;
using EmployeeManagement.Server.Data;
using EmployeeManagement.Server.Employees;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EmployeeDb")));

builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// RFC 9457 problem+json bodies for API errors; OpenAPI document served at /openapi/v1.json.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Employee Management API v1"));
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Browser requests for unknown pages re-execute to the Blazor "not found" page;
// requests under /api opt out and always get JSON problem details instead.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseApiErrorResponses();

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapEmployeeEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EmployeeManagement.Client._Imports).Assembly);

app.Run();

// Exposes the implicit Program class to WebApplicationFactory<Program> in integration tests.
public partial class Program;