using EmployeeManagement.Contracts.Employees;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Server.Employees;

// Minimal API routes for /api/employees. Each handler only translates between HTTP and IEmployeeService.
public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees")
            .WithTags("Employees")
            // Keep API errors as JSON: never re-execute them to the HTML "not found" page.
            .WithMetadata(new SkipStatusCodePagesAttribute());

        group.MapGet("/", GetAll).WithName("GetEmployees").WithSummary("List all employees.");
        group.MapGet("/{id:int}", GetById).WithName("GetEmployee").WithSummary("Get one employee.");
        group.MapPost("/", Create).WithName("CreateEmployee").WithSummary("Create an employee.");
        group.MapPut("/{id:int}", Update).WithName("UpdateEmployee").WithSummary("Replace an employee's details.");
        group.MapDelete("/{id:int}", Delete).WithName("DeleteEmployee").WithSummary("Delete an employee and their address.");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<EmployeeDto>>> GetAll(IEmployeeService service, CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct));

    private static async Task<Results<Ok<EmployeeDto>, NotFound<ProblemDetails>>> GetById(
        int id, IEmployeeService service, CancellationToken ct)
    {
        var employee = await service.GetByIdAsync(id, ct);
        return employee is null ? NotFoundProblem(id) : TypedResults.Ok(employee);
    }

    private static async Task<Results<Created<EmployeeDto>, ValidationProblem, Conflict<ProblemDetails>>> Create(
        EmployeeRequest request, IEmployeeService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.Outcome switch
        {
            EmployeeOutcome.Success => TypedResults.Created($"/api/employees/{result.Employee!.Id}", result.Employee),
            EmployeeOutcome.ValidationFailed => TypedResults.ValidationProblem(result.Errors!),
            EmployeeOutcome.DuplicateEmail => DuplicateEmailProblem(request.Email),
            _ => throw new InvalidOperationException($"Unexpected outcome {result.Outcome} for create."),
        };
    }

    private static async Task<Results<Ok<EmployeeDto>, ValidationProblem, NotFound<ProblemDetails>, Conflict<ProblemDetails>>> Update(
        int id, EmployeeRequest request, IEmployeeService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.Outcome switch
        {
            EmployeeOutcome.Success => TypedResults.Ok(result.Employee!),
            EmployeeOutcome.ValidationFailed => TypedResults.ValidationProblem(result.Errors!),
            EmployeeOutcome.NotFound => NotFoundProblem(id),
            EmployeeOutcome.DuplicateEmail => DuplicateEmailProblem(request.Email),
            _ => throw new InvalidOperationException($"Unexpected outcome {result.Outcome} for update."),
        };
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>>> Delete(
        int id, IEmployeeService service, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? TypedResults.NoContent() : NotFoundProblem(id);

    private static NotFound<ProblemDetails> NotFoundProblem(int id) => TypedResults.NotFound(new ProblemDetails
    {
        Title = "Employee not found.",
        Detail = $"No employee exists with id {id}.",
        Status = StatusCodes.Status404NotFound,
    });

    private static Conflict<ProblemDetails> DuplicateEmailProblem(string? email) => TypedResults.Conflict(new ProblemDetails
    {
        Title = "Email address already in use.",
        Detail = $"Another employee already uses the email address '{email?.Trim()}'.",
        Status = StatusCodes.Status409Conflict,
    });
}