# Employee Management

Employee management app built with .NET 10, ASP.NET Core Web API, Blazor WebAssembly, EF Core (Code First) and SQLite.

> Work in progress — sections marked TODO are filled in as features land.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (pinned via `global.json`)
- No database install needed — on startup the app applies EF Core migrations, which create `src/EmployeeManagement.Server/employees.db` and insert 5 seed employees. Delete that file to reset the data.

## Build, run, test

```powershell
dotnet restore
dotnet build
dotnet run --project src/EmployeeManagement.Server
dotnet test
```

- UI: the URL printed by `dotnet run` (e.g. `http://localhost:5233`)
- VS Code: press **F5** and pick **Run Server (C# Dev Kit)** (or the `coreclr` fallback) from `.vscode/launch.json`. `Ctrl+Shift+B` builds; the `watch` task runs `dotnet watch`.
- Swagger UI: `/swagger` — TODO

## Solution layout

| Project | Purpose |
|---|---|
| `src/EmployeeManagement.Server` | Single host: REST API, EF Core + SQLite, serves the Blazor client |
| `src/EmployeeManagement.Client` | Blazor WebAssembly UI — talks to the API over HTTP only |
| `src/EmployeeManagement.Contracts` | Request/response DTOs shared by API and UI |
| `tests/EmployeeManagement.Tests` | xUnit unit + integration tests |

The Client project has no reference to the Server project or EF Core, so the UI *cannot* access the database directly — the "UI goes through the API" rule is enforced by the project graph.

## Architecture overview

TODO

## Assumptions and technical decisions

- Blazor WebAssembly hosted by the API project, rather than Interactive Server, so the UI is a true API client.
- Host project renamed from the template default `Web` to `Server` so it pairs clearly with `Client` (Web suggested it might be the UI).
- **EF Core Code First with migrations** (`Data/Migrations`), applied automatically at startup via `Database.Migrate()`. Chosen over `EnsureCreated()` because migrations can evolve the schema later without dropping data.
- **Seed data via `HasData`**, so the 5 employees/addresses are part of the initial migration.
- **One-to-one Employee → Address**: `Addresses.EmployeeId` is a unique foreign key with `ON DELETE CASCADE`, so deleting an employee deletes their address in the database itself.
- **Case-insensitive unique email**: the `Email` column uses SQLite's `NOCASE` collation plus a unique index, so the database rejects `Jane@x.com` if `jane@x.com` exists. (NOCASE only folds ASCII letters — acceptable for email addresses here.)
- **Date of birth instead of age (deliberate deviation):** the spec's add form lists *age*. A stored age goes stale every birthday, so `Employee` stores `DateOfBirth` (`DateOnly`, saved as `yyyy-MM-dd` text in SQLite) and the form collects date of birth. Age can be calculated from it whenever it's needed. This was changed through a second migration (`ReplaceAgeWithDateOfBirth`) rather than editing the first one, so existing databases upgrade in place.
- **Contracts: separate input and output types.** `EmployeeDto`/`AddressDto` (responses) are immutable records. `EmployeeRequest`/`AddressRequest` (POST/PUT bodies) are mutable classes so the Blazor form can bind straight to them. Database entities never leave the Server project.
- **One request type for create and update.** POST and PUT take the same fields; PUT gets the employee id from the URL, so a separate `UpdateEmployeeRequest` would be an identical copy.
- **Validation rules live once, in Contracts** (DataAnnotations attributes), so the form and the API enforce the same rules. `EmployeeRequestValidator` runs them, including the nested address, because .NET's built-in `Validator` doesn't descend into nested objects. Errors come back keyed by field (`Email`, `Address.Zip`), the shape ASP.NET Core uses for a 400 validation response.
- **Validation details:** phone and ZIP use `[0-9]` rather than `\d` (in .NET, `\d` also matches non-ASCII digits); state must be one of the 50 state codes or DC, accepted in any case; date of birth must be between 1900-01-01 and today; email uses .NET's `[EmailAddress]` check, which is deliberately loose.- **Pinned package versions** (no `10.*` wildcards) so a fresh clone restores exactly what was tested.
- TODO

## Incomplete requirements

TODO

## What I'd improve with more time

TODO