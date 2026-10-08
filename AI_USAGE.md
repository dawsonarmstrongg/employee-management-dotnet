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
| 8 | Make every `/api` error JSON, including routing 404/405s. | Middleware replacing the per-route opt-out; re-tested 404/405/400/409 under `/api` and the HTML Not Found page for browser URLs. |
| 9 | Build the Blazor page: employee table with combined Name/Address columns, add form with field-level validation and API error display. | Page + table/form components, typed API client, shared-validation form validator, prerendering off. Verified in a real browser (headless Edge via Playwright, from a throwaway environment outside the repo): 5 seeded rows load; empty submit shows 9 messages and sends no request; a bad phone shows its message after editing; a valid add with " va"/blank Address 2 appears as `VA` with no blank segment and clears the form; a duplicate email in different casing shows the API's 409 message under Email. |
| 10 | Format the phone number automatically while typing, not only after leaving the field. | `InputPhoneNumber` component + `PhoneNumberFormatter`. Tested key by key in a browser: each digit reformats immediately, an 11th digit is ignored, Backspace works, letters and dots are stripped, pasted `+1 (555) 222-3333` becomes `(555)-222-3333`, and a saved employee stores the formatted number. |
| 11 | Turn the State field into a dropdown that filters as I type the 2-letter code. | `InputUsState` combobox, shared `UsStates` list in Contracts (also used by `[UsState]`) and `UsStateFilter`. Tested in a browser: focus lists 51 options; `T` → TN, TX; `TE` matches by name; arrow keys + Enter select without submitting the form; click and Escape work; digits are stripped and lowercase is uppercased; `ZZ` shows the error, which clears on `TX`; a saved employee stores `TX` and the form resets. |
| 12 | Apply my Zelis "Lumen" design brief to the whole app (summarized [below](#ui-design-prompt-summary)). Before building I narrowed it: sorting only, keep the Date of birth column, sort Name by last name and show it "Last, First". | One token file and theme, a shared component set in `Components/UI/` (11 components), a new app shell, and a redesigned employees page (sortable table, add form in a drawer with a discard check, toasts, loading/empty/error states), plus redesigned Not Found and error pages. No business logic, API calls or validation rules changed. Verified with a Playwright script in headless Edge at desktop, tablet and phone widths (85 checks: sizes and colors, sorting, keyboard and focus, validation, the 409, phone/state inputs, saving, every page state), axe-core (0 WCAG 2.2 AA violations on every page and state), and a review of every screenshot. |
| | TODO — add more as work progresses | |

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

**My scope decisions:** sorting only, because search, filtering and pagination are out of scope in the exercise. I kept the Date of birth column, and Name is sorted by last name and shown "Last, First".

**Not built, and why:** dashboard cards, charts, tabs, breadcrumbs and overflow menus (no screen needs them); the logo (no official asset is in the repo, so the header shows the app name and has a slot for one); Avenir (a licensed font, so it isn't bundled).

## Where AI was useful

- Turning the requirements doc into a checklist and project layout.
- Driving the `dotnet` CLI for scaffolding and wiring references/packages.
- Turning a long design brief into a token file, a theme and reusable components, then checking the result with scripted browser and accessibility tests at three screen sizes.
- TODO

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
- TODO

## How I reviewed and tested AI-generated code

- Read every diff before committing.
- Built after each step; TODO — tests, Swagger checks, manual UI checks.
- UI redesign: scripted browser checks (Playwright, headless Edge) at desktop, tablet and phone widths, axe-core accessibility scans, and a look at every screenshot. The screenshots caught six layout bugs that the scripted checks and axe missed (listed above).