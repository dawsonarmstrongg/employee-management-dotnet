using EmployeeManagement.Contracts.Employees;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EmployeeManagement.Server.Employees;

// Minimal API routes for /api/employees. Each handler only translates between HTTP and IEmployeeService.
public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/employees").WithTags("Employees");


        group.MapGet("/", GetAll).WithName("GetEmployees").WithSummary("List all employees.");
        group.MapGet("/{id:int}", GetById).WithName("GetEmployee").WithSummary("Get one employee.")
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create).WithName("CreateEmployee").WithSummary("Create an employee.")
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:int}", Update).WithName("UpdateEmployee").WithSummary("Replace an employee's details.")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:int}", Delete).WithName("DeleteEmployee").WithSummary("Delete an employee and their address.")
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<EmployeeDto>>> GetAll(IEmployeeService service, CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct));

    private static async Task<Results<Ok<EmployeeDto>, ProblemHttpResult>> GetById(
        int id, IEmployeeService service, CancellationToken ct)
    {
        var employee = await service.GetByIdAsync(id, ct);
        return employee is null ? NotFoundProblem(id) : TypedResults.Ok(employee);
    }

    private static async Task<Results<Created<EmployeeDto>, ValidationProblem, ProblemHttpResult>> Create(
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

    private static async Task<Results<Ok<EmployeeDto>, ValidationProblem, ProblemHttpResult>> Update(
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

    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
        int id, IEmployeeService service, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? TypedResults.NoContent() : NotFoundProblem(id);

    // TypedResults.Problem sends application/problem+json (RFC 9457), the same as the 400 validation errors.
    // Its status code isn't part of the return type, so the routes above declare 404/409 for the OpenAPI document.
    private static ProblemHttpResult NotFoundProblem(int id) => TypedResults.Problem(
        title: "Employee not found.",
        detail: $"No employee exists with id {id}.",
        statusCode: StatusCodes.Status404NotFound);

    private static ProblemHttpResult DuplicateEmailProblem(string? email) => TypedResults.Problem(
        title: "Email address already in use.",
        detail: $"Another employee already uses the email address '{email?.Trim()}'.",
        statusCode: StatusCodes.Status409Conflict);
}