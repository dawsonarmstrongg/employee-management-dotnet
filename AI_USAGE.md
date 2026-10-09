# AI Usage

## Tools

- **GitHub Copilot CLI** (agent mode) using **Claude Opus** — planning, scaffolding, implementation, throwaway browser checks, docs, and checking the QA sub-agent's work.
- **Claude Sonnet 5 as a separate QA sub-agent**, started from the same Copilot CLI session with fresh context. It wrote and ran the automated test suite (see [QA sub-agent](#qa-sub-agent-summary)).

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
| 8 | Make every `/api` error JSON, including routing 404/405s. | Middleware replacing the per-route opt-out; re-tested 404/405/400/409 under `/api` and the HTML Not Found page for browser URLs. |
| 9 | Build the Blazor page: employee table with combined Name/Address columns, add form with field-level validation and API error display. | Page + table/form components, typed API client, shared-validation form validator, prerendering off. Verified in a real browser (headless Edge via Playwright, from a throwaway environment outside the repo): 5 seeded rows load; empty submit shows 9 messages and sends no request; a bad phone shows its message after editing; a valid add with " va"/blank Address 2 appears as `VA` with no blank segment and clears the form; a duplicate email in different casing shows the API's 409 message under Email. |
| 10 | Format the phone number automatically while typing, not only after leaving the field. | `InputPhoneNumber` component + `PhoneNumberFormatter`. Tested key by key in a browser: each digit reformats immediately, an 11th digit is ignored, Backspace works, letters and dots are stripped, pasted `+1 (555) 222-3333` becomes `(555)-222-3333`, and a saved employee stores the formatted number. |
| 11 | Turn the State field into a dropdown that filters as I type the 2-letter code. | `InputUsState` combobox, shared `UsStates` list in Contracts (also used by `[UsState]`) and `UsStateFilter`. Tested in a browser: focus lists 51 options; `T` → TN, TX; `TE` matches by name; arrow keys + Enter select without submitting the form; click and Escape work; digits are stripped and lowercase is uppercased; `ZZ` shows the error, which clears on `TX`; a saved employee stores `TX` and the form resets. |
| 12 | Apply my Zelis "Lumen" design brief to the whole app (summarized [below](#ui-design-prompt-summary)). Before building I narrowed it: sorting only, keep the Date of birth column (later removed, see [below](#a-requirement-the-ai-missed-table-columns)), sort Name by last name and show it "Last, First". | One token file and theme, a shared component set in `Components/UI/` (11 components), a new app shell, and a redesigned employees page (sortable table, add form in a drawer with a discard check, toasts, loading/empty/error states), plus redesigned Not Found and error pages. No business logic, API calls or validation rules changed. Verified with a Playwright script in headless Edge at desktop, tablet and phone widths (85 checks: sizes and colors, sorting, keyboard and focus, validation, the 409, phone/state inputs, saving, every page state), axe-core (0 WCAG 2.2 AA violations on every page and state), and a review of every screenshot. |
| 13 | Write a Lead QA Tester prompt and give it, with the exercise document, to a separate Sonnet sub-agent with fresh context. It may change only test files (summarized [below](#qa-sub-agent-summary)). | 98 tests (159 cases) across unit, API integration and Blazor component (bUnit) tests, each tagged with the requirement it checks, plus exploratory API, browser and accessibility checks and a written report. 158 passed and 1 failed on purpose, exposing a real inconsistency: 404 and 409 errors were sent as `application/json`. I checked every finding before acting on it: one was confirmed and fixed, one was a false alarm, and my review of the new tests found two more problems (see the corrections below). All 159 now pass. |
| 14 | Test a fresh clone of the repo, following only the README, to confirm it runs as-is. | Cloned the repo from GitHub into an empty folder and ran restore, build, test and run exactly as the README says: 0 warnings, 159/159 tests passed, the database was created and seeded with 5 employees, and the page, Swagger and API all responded with no browser console errors. |

## UI design prompt (summary)

The redesign came from one long prompt I wrote. In short, it asked for:

- **Goal:** make the app look like a production OneZelis product in the Zelis "Lumen" design language: a calm, information-first enterprise UI rather than a generic Bootstrap or startup template, applied to every screen.
- **Keep behavior:** no changes to business logic, API calls, validation, routing or data contracts; no new framework or large UI library; styles in one reusable theme instead of one-off values.
- **Brand tokens:** one central file for colors (Ink Blue as the anchor, Royal Purple for secondary structure, Bright Blue for links, Solar Yellow rarely and never with white text), success/warning/error/info colors, borders and purple-tinted shadows.
- **Type and layout:** Avenir Next with Segoe UI as the fallback and a set type scale; a 64px header and a 248px side menu that becomes an icon rail on tablets and a slide-out menu on phones; 8px spacing steps; fixed corner radii.
- **Components:** buttons, forms (labels above fields, required markers, inline errors), data tables (tinted header, 40–44px rows, sort indicators), status chips with text and an icon, alerts, toasts, modals and drawers, tooltips, and loading, empty and error states.
- **Accessibility:** WCAG 2.2 AA, full keyboard use, a 3px Ink Blue focus ring, 44px touch targets, reduced-motion support, and never color alone.
- **Avoid:** gradients, translucent panels, heavy shadows, emoji icons, imitation logos, yellow primary buttons.
- **Finish:** review every page, then summarize the files, tokens and components changed.

**My scope decisions:** sorting only, because search, filtering and pagination are out of scope in the exercise. I kept the Date of birth column (a mistake I later reversed; see [below](#a-requirement-the-ai-missed-table-columns)), and Name is sorted by last name and shown "Last, First".

**Not built, and why:** dashboard cards, charts, tabs, breadcrumbs and overflow menus (no screen needs them); the logo (no official asset is in the repo, so the header shows the app name and has a slot for one); Avenir (a licensed font, so it isn't bundled).

## QA sub-agent (summary)

For an independent test pass I wrote a "Lead QA Tester" prompt and ran it as a separate **Claude Sonnet 5** sub-agent, started from the Copilot CLI session.

- **Why fresh context:** the sub-agent had no memory of how the app was built. It saw only the QA prompt, the exercise document and the repo, so it tested the app against the brief the way a tester who didn't write it would, instead of sharing the assumptions of the model that wrote the code. Using a different model also gave a second opinion.
- **Guardrails in the prompt:** change only files under `tests\` and never fix the app, not even one line; a test that exposes a bug stays failing; git is read-only; don't touch my `employees.db` (tests and app runs use their own temporary database); pinned packages only, with bUnit approved and nothing else added; don't delete files; stop only processes it started; don't post to Jira or GitHub.
- **What it delivered:** the test suite in `tests/` (described in the README's Tests section), exploratory checks of the API, the browser UI and accessibility, and a report that traced each requirement to its tests and listed findings with severity and evidence.
- **How I checked it:** confirmed that only `tests\` had changed and that `employees.db` was untouched, re-ran the build and tests myself, and reproduced each finding before acting on it:

| Finding | Verdict | What I did |
|---|---|---|
| 404 and 409 errors were sent as `application/json`, while 400s used `application/problem+json` | Confirmed in the code and with live requests | Fixed: both now use `TypedResults.Problem`, and the routes list them with `.ProducesProblem(...)` so Swagger still shows them. The failing test now passes. |
| The phone field went blank during a browser test | False alarm: its script looked for `#phoneNumber`, but the field's id is `phone`, so it never typed a phone number | Re-ran the scenario with the right selector, plus typing, Ctrl+V paste and autofill: all correct. No app change. |
| (my review) Each test run left 5 temporary database files behind | Confirmed | Fixed in the test setup (see the corrections below). |
| (my review) Some requirement tags pointed at the wrong requirement, for example duplicate-email tests tagged as the phone requirement | Confirmed | Corrected the tags; the README now lists what each ID means. |

## Where AI was useful

- Turning the requirements doc into a checklist and project layout.
- Driving the `dotnet` CLI for scaffolding and wiring references/packages.
- Turning a long design brief into a token file, a theme and reusable components, then checking the result with scripted browser and accessibility tests at three screen sizes.
- An independent test pass: a sub-agent with fresh context turned the brief into requirement-tagged tests and found a real inconsistency in the API's error responses.
- Learning as I built. This is my first web app, so I asked the AI to explain each piece in plain language before moving on: how a request reaches the API and where its status code comes from, what Playwright is and how it differs from the tests in the repo, and a guided walkthrough of the code. Asking how status codes are decided is what exposed the routing 404/405 gap listed below.

## Where AI output needed correction

- **Template layout:** `dotnet new blazor` produced a nested `src/EmployeeManagement.Web/EmployeeManagement.Web/` folder plus a stray `.sln`, and named the client `EmployeeManagement.Web.Client`. I had it flatten the layout, rename the client project, and fix namespaces/references before the first commit.
- **Stale build during verification:** the first verification run used `--no-build` after the migration was added, so the app reported "No migrations were found" and created an empty database. Caught by reading the startup log; rebuilt and re-ran.
- **Verification script bug:** the AI's first throwaway check script didn't compile (it used record `with` syntax on a class). Fixed the script; the Contracts code itself was unaffected.
- **VS Code F5 build failure (MSB4018 ApplyCompressionNegotiation, 'item with the same key').** My terminal builds used `C:\` and passed, so I initially reported a clean build. VS Code passes the path as `c:\`, and with that casing the SDK registered the Client's template `wwwroot/appsettings*.json` twice, which crashed static-asset compression. I reproduced it with a lowercase-path build and removed the two unused template files, then verified the build with both casings plus an app run.
- **API errors vs. the HTML "not found" page:** the template sends every error response with no body to the Blazor Not Found page, which would turn an API 404 into HTML. The AI's first fix split the pipeline with `UseWhen`, which kept the API as JSON but broke the HTML Not Found page for unknown browser URLs. Caught by testing an unknown URL; replaced it with the built-in `SkipStatusCodePages` metadata on the API route group. That still missed errors raised by routing before an endpoint is chosen (`/api/employees/abc` returned the HTML page; a 405 had an empty body), which I spotted while having the AI explain how status codes are decided. Final fix: one small middleware (`ApiErrorResponses.cs`) that turns off the HTML page for everything under `/api` and adds a problem details body to errors that have none. Re-tested every case.
- **Invented API name:** the AI wrote `InteractiveWebAssemblyNoPrerender`, which doesn't exist. The build caught it; replaced with `new InteractiveWebAssemblyRenderMode(prerender: false)`.
- **Phone input, first version:** the browser test showed two problems. (1) The input's 14-character limit cut off a pasted `+1 (555) 222-3333` before formatting, giving `(155)-522-23`; I removed the limit because the formatter already stops at 10 digits. (2) Validating on every keystroke showed "invalid format" after the first digit; validation now waits until the number is complete or the field loses focus.
- **Browser validation got in first:** the original form's email input (`type="email"`) let the browser's built-in check block the submit with its own popup (for example, for an address without `@`), so the form's own message and focus handling never ran. Added `novalidate`; the shared validator and the API still enforce every rule.
- **Error page could never appear:** `App.razor` gave every request the interactive WebAssembly render mode, so when the server's error handler re-ran a failed request as `/Error`, the browser app took over and the error page never showed. Found while redesigning that page; fixed with `[ExcludeFromInteractiveRouting]` on it and a render mode chosen per request. Verified `/Error` renders as static HTML.
- **Focus lost after clicking outside a dialog:** the first dialog script returned focus to whatever had focus when the dialog opened. When the discard confirmation was opened by clicking outside the drawer, that was the page `<body>`, so keyboard focus was lost. Caught by the scripted focus checks; focus now goes back to the dialog underneath, or to the page heading.
- **Layout bugs only visible in screenshots** (all passed the scripted checks and axe): the form's section headings ("Contact", "Address") sat on the divider line because the theme un-floated the `<legend>`; "Address 2(optional)" was missing a space; the success message covered the Refresh and Add buttons for 6 seconds (moved to the bottom right); the mobile menu panel ended after its last link instead of filling the screen; names wrapped onto two lines on phones; and the hidden skip link's shadow showed as a faint strip at the top of the mobile menu.
- **Test script mistakes, not app bugs:** the first browser script expected the wrong oldest employee for the Date of birth sort, expected the first Tab to reach the skip link (Blazor intentionally moves focus to the page heading after loading; the skip link is reached with Shift+Tab), released a held network request in the wrong order, and read a tooltip during its fade-in. Each failure was investigated before the script was changed.
- **QA false alarm:** the QA sub-agent reported that the phone field went blank in a browser test and flagged a possible bug. Its own screenshot showed the field had never been filled: the script searched for `#phoneNumber`, but the field's id is `phone`. The same mistake meant its browser duplicate-email check never reached the server. I re-ran both with the right selector (both passed) and rejected the finding.
- **Test cleanup that silently failed:** the QA sub-agent's test setup deleted its temporary database after each test class, but SQLite's connection pool still had the file open, so the delete failed without an error and every run left 5 files in `%TEMP%`. Fixed by turning off pooling in the tests' connection string (`Pooling=False`); a run now leaves nothing behind.
- **Wrong requirement tags:** the QA sub-agent labeled some tests with requirement IDs that didn't match its own list (duplicate-email tests marked as the phone requirement, add-form tests as Swagger, delete tests as seed data). Found while documenting the `--filter` option; corrected the tags and re-ran the filter for every ID.
- **Sub-agent stopped by my own chat messages:** the first two QA runs were started in the background and stopped partway, with no error, shortly after I sent a new chat message; the CLI cancelled the background sub-agent when the new message arrived. The third run was started in the foreground with a handover note describing the partial work, and finished.

## A requirement the AI missed: table columns

The brief says the table should display "these columns **only**": Name, Email, Phone and Address. The AI's first version of the table (commit `455fed8`) added a fifth column, Date of birth, and neither the commit message nor its summary mentioned it. After that, every review treated the extra column as a feature instead of a requirement miss:

- During the redesign I chose to keep it, without checking it against the brief.
- The QA sub-agent tagged the table tests with the columns requirement (R-15) but wrote "plus the documented extra Date of birth column", so it accepted the extra column instead of failing it.
- The README listed it as a "deliberate deviation", and an interviewer-style review by another sub-agent marked it "Deviation" rather than "Not met".

I caught it myself while re-reading the brief before submitting. I removed the column, its sort option and its test, and the loading placeholder now shows four columns. Date of birth is still collected on the form and stored, because that deviation (instead of age) has a real reason behind it.

**Lesson:** an AI reviewer that reads the existing docs tends to accept what they already say. Requirement words like "only" need a check against the brief itself, line by line, not against the code or the README.

## How I reviewed and tested AI-generated code

- Read every diff before committing.
- Built after each step. Since the QA pass, `dotnet test` (158 cases since the Date of birth sort test was removed) must pass before a commit. Checked in Swagger that every endpoint lists its status codes, and ran the app myself (F5 in VS Code) to try each feature by hand.
- Independent QA pass by a separate sub-agent (above). I treated its report as claims to check, not facts: I reproduced each finding before changing anything, which caught one false alarm.
- Before submitting: a fresh clone from GitHub run exactly as the README describes, and a CI workflow that builds and tests on Ubuntu and Windows for every push.
- UI redesign: scripted browser checks (Playwright, headless Edge) at desktop, tablet and phone widths, axe-core accessibility scans, and a look at every screenshot. The screenshots caught six layout bugs that the scripted checks and axe missed (listed above).