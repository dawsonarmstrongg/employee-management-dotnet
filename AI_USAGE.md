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
| | TODO — add more as work progresses | |

## Where AI was useful

- Turning the requirements doc into a checklist and project layout.
- Driving the `dotnet` CLI for scaffolding and wiring references/packages.
- TODO

## Where AI output needed correction

- **Template layout:** `dotnet new blazor` produced a nested `src/EmployeeManagement.Web/EmployeeManagement.Web/` folder plus a stray `.sln`, and named the client `EmployeeManagement.Web.Client`. I had it flatten the layout, rename the client project, and fix namespaces/references before the first commit.
- **Stale build during verification:** the first verification run used `--no-build` after the migration was added, so the app reported "No migrations were found" and created an empty database. Caught by reading the startup log; rebuilt and re-ran.
- TODO

## How I reviewed and tested AI-generated code

- Read every diff before committing.
- Built after each step; TODO — tests, Swagger checks, manual UI checks.