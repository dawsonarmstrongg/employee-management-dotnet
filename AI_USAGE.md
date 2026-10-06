# AI Usage

## Tools

- **GitHub Copilot CLI** (agent mode) using **Claude Opus** — planning, scaffolding, implementation, tests, docs.

## Representative prompts and outcomes

| # | Prompt (summary) | Outcome |
|---|---|---|
| 1 | Summarize the exercise and list the deliverables; should this be a separate repo? | Deliverable checklist and recommended project layout. |
| 2 | Scaffold the solution skeleton. | Generated solution, 4 projects, references and packages via `dotnet` CLI. |
| 3 | Rename the Web project to Server. | Folder/csproj/namespace/test-reference rename; also caught the scoped-CSS bundle name in `App.razor`. |
| 4 | Create the data layer: entities, DbContext, one-to-one with cascade delete, case-insensitive unique email, seed data, migrations. | Entities + `AppDbContext` + `InitialCreate` migration; verified schema, seed rows, unique-email and cascade behavior directly against the SQLite file. |
| 5 | Replace Age with DateOfBirth (my decision: stored ages go stale). | New migration `ReplaceAgeWithDateOfBirth`; verified it both upgrades the existing database in place and builds a fresh one from scratch, and that the email collation, unique index and cascade FK survived SQLite's table rebuild. |
| 6 | Build the Contracts: response DTOs, request types and shared validation rules. | DTOs, request classes, custom `[UsState]`/`[DateOfBirth]` attributes and a validator that also checks the nested address. Verified 22 valid/invalid cases (phone formats, non-ASCII digits, states, ZIPs, dates, empty form) and the JSON shape with a throwaway script before committing. |
| 7 | Build the CRUD API as Minimal APIs with Swagger, returning 201/400/404/409/204. | Service + endpoints + OpenAPI/Swagger UI. Verified every endpoint and status code with curl against a scratch database (including case-insensitive duplicate email on POST and PUT, trimming/uppercasing, cascade delete leaving no orphan addresses) and that Swagger lists each endpoint's status codes. |
| | TODO — add more as work progresses | |

## Where AI was useful

- Turning the requirements doc into a checklist and project layout.
- Driving the `dotnet` CLI for scaffolding and wiring references/packages.
- TODO

## Where AI output needed correction

- **Template layout:** `dotnet new blazor` produced a nested `src/EmployeeManagement.Web/EmployeeManagement.Web/` folder plus a stray `.sln`, and named the client `EmployeeManagement.Web.Client`. I had it flatten the layout, rename the client project, and fix namespaces/references before the first commit.
- **Stale build during verification:** the first verification run used `--no-build` after the migration was added, so the app reported "No migrations were found" and created an empty database. Caught by reading the startup log; rebuilt and re-ran.
- **Verification script bug:** the AI's first throwaway check script didn't compile (it used record `with` syntax on a class). Fixed the script; the Contracts code itself was unaffected.
- **VS Code F5 build failure (MSB4018 ApplyCompressionNegotiation, 'item with the same key').** My terminal builds used `C:\` and passed, so I initially reported a clean build. VS Code passes the path as `c:\`, and with that casing the SDK registered the Client's template `wwwroot/appsettings*.json` twice, which crashed static-asset compression. I reproduced it with a lowercase-path build and removed the two unused template files, then verified the build with both casings plus an app run.
- **API errors vs. the HTML "not found" page:** the template sends every error response with no body to the Blazor Not Found page, which would turn an API 404 into HTML. The AI's first fix split the pipeline with `UseWhen`, which kept the API as JSON but broke the HTML Not Found page for unknown browser URLs. Caught by testing an unknown URL; replaced with the built-in `SkipStatusCodePages` metadata on the API route group and re-tested both.
- TODO

## How I reviewed and tested AI-generated code

- Read every diff before committing.
- Built after each step; TODO — tests, Swagger checks, manual UI checks.