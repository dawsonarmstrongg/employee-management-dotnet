# AI Usage

## Tools

- **GitHub Copilot CLI** (agent mode) using **Claude Opus** — planning, scaffolding, implementation, tests, docs.

## Representative prompts and outcomes

| # | Prompt (summary) | Outcome |
|---|---|---|
| 1 | Summarize the exercise and list the deliverables; should this be a separate repo? | Deliverable checklist and recommended project layout. |
| 2 | Scaffold the solution skeleton. | Generated solution, 4 projects, references and packages via `dotnet` CLI. |
| | TODO — add more as work progresses | |

## Where AI was useful

- Turning the requirements doc into a checklist and project layout.
- Driving the `dotnet` CLI for scaffolding and wiring references/packages.
- TODO

## Where AI output needed correction

- **Template layout:** `dotnet new blazor` produced a nested `src/EmployeeManagement.Web/EmployeeManagement.Web/` folder plus a stray `.sln`, and named the client `EmployeeManagement.Web.Client`. I had it flatten the layout, rename the client project, and fix namespaces/references before the first commit.
- TODO

## How I reviewed and tested AI-generated code

- Read every diff before committing.
- Built after each step; TODO — tests, Swagger checks, manual UI checks.