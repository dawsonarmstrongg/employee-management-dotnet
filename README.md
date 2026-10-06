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
- **Pinned package versions** (no `10.*` wildcards) so a fresh clone restores exactly what was tested.
- TODO

## Incomplete requirements

TODO

## What I'd improve with more time

TODO