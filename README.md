# Expense Management

## Overview

This project provides an expense management solution for administrators who manage company users and their expenses.

The application uses .NET Aspire to orchestrate a PostgreSQL database, an ASP.NET Core Web API, and an Angular frontend.

## Technical Prerequisites

- Docker Desktop or an equivalent container runtime
- .NET 9 SDK with ASP.NET Core
- Node.js >= 22.12

## Running The Application

Start the Aspire project located in `backend\src\AppHost`.

Aspire starts the database, the backend, and the frontend. The `dotnet` console displays the Aspire dashboard URL where the application services can be inspected.

### Specification

The application is an expense management tool used by an administrator to manage company users, monthly expense reports, and expense entries. End users do not submit their own expenses in the application; the administrator is responsible for creating users, creating reports, and entering expenses on behalf of users.

#### Backend Scope

The backend exposes an ASP.NET Core Web API backed by PostgreSQL and orchestrated locally through .NET Aspire.

The API supports user management with the following behavior:
- create users with first name, last name, postal address, monthly expense quota, and active status;
- list users for administrative views;
- list only assignable users when creating expense reports;
- update user information through the backend API;
- activate and deactivate users through domain rules;
- logically delete users so historical expense report references remain valid;
- reject invalid user data, including invalid French postal codes and invalid monthly quotas.

The API supports expense report management with the following behavior:
- create one monthly expense report for one assigned user;
- reject report creation when the selected user is inactive, deleted, or otherwise not assignable;
- reject duplicate reports for the same user and calendar month;
- compute report titles from the assigned user and report period using the `User - Month Year` format;
- expose report listing and report detail retrieval;
- physically delete an expense report together with its attached expense entries;
- keep the report period as explicit `year` and `month` values to avoid timezone ambiguity.

The API supports expense entry management with the following behavior:
- create expense entries attached to an existing expense report;
- store each expense with a business date, description, positive EUR amount, and billing address;
- exchange expense dates as ISO `yyyy-MM-dd` date-only values;
- enforce the 50-character description limit;
- enforce French postal code validation on billing addresses;
- reject expenses whose date is outside the report month;
- enforce the assigned user's monthly expense quota when adding active entries;
- count only active expense entries toward quota and pagination totals;
- update existing expense entries while preserving report-month consistency;
- logically delete individual expense entries;
- list active entries for a report with a page size of 5.

Backend errors are returned through stable API error codes and mapped to explicit HTTP responses. Domain rules remain protected inside domain objects and application services, while persistence concerns stay behind repositories and unit-of-work boundaries.

#### Frontend Scope

The Angular frontend implements the primary administrator workflows needed to operate the delivered backend behavior.

The frontend supports user workflows with:
- a user list page;
- a user creation form using Reactive Forms;
- local validation for required fields, postal code format, and positive monthly quota;
- API error display and automatic list refresh after successful creation.

The frontend supports expense report workflows with:
- a report list page;
- a report creation form based on assignable users and a selected month/year;
- duplicate-report and backend validation error display;
- navigation from the report list to a report detail page;
- report deletion from the detail page with an explicit confirmation because attached expenses are permanently removed with the report.

The frontend supports expense entry workflows with:
- report detail loading with title, assigned user, and period summary;
- active expense listing in pages of 5 entries;
- pagination controls and loading, empty, and error states;
- an expense creation form using Reactive Forms;
- local validation for required fields, amount, description length, and postal code format;
- business error display for monthly quota and date-outside-report-month failures;
- list refresh after successful expense creation.

## Implementation Notes

GENERAL NOTES

I made the choice for more dedicated focus on backend architecture and good practices, and I restricted the frontend area to key functionalities showing relevant design and implementation decisions.
Reason: keeping the implementation focused on the current delivery scope.

Backend: Implementation assumptions and choices
- Architecture: one WebApi project with hexagonal folders, avoiding unnecessary assemblies.
- Domain rules live in domain objects and application services, not controllers or EF mappings.
- API endpoints expose DTOs only; EF/domain entities are never returned directly.
- Repositories materialize data; the API never exposes `IQueryable`.
- CQRS pattern for better read/write segmentation.
- Unit of Work for more robust isolation and transaction management.
- Domain and application errors use explicit exception types and stable API error codes.
- Business dates are calendar values; audit timestamps are UTC instants.
- Expense report periods use `year` and `month` to avoid timezone ambiguity.
- The README is interpreted as one report per user and calendar month.
- User deletion is logical to preserve historical references.
- Inactive users cannot be assigned to new reports, but existing report edits are not blocked.
- Individual expense deletion is logical, as required by the functional requirements.
- Report deletion is ambiguous; the working assumption is physical deletion with attached expenses.
- Report deletion currently follows the product rule by physically deleting attached expenses; production rules should clarify archival, audit and legal retention requirements.
- Expense amounts are planned as a domain money value with EUR by default.
- Billing address data belongs to expenses because the functional requirements include invoice address details.
- FluentValidation validates inputs; domain objects still protect invariants.
- Swagger UI is kept for local API discovery.
- Swagger is exposed by the `webapi` resource, not by the AppHost dashboard
  => Use the WebApi endpoint shown in Aspire Resources then append `/swagger/index.html`.
- Aspire is the local orchestration path for PostgreSQL and runtime validation.
- Backend tests with one test project, organized by layer and context, to keep coverage simple and readable.
- Security/authentication is not implemented because it is outside the current functional scope.
- EF Core design-time commands use `ConnectionStrings__database` from the environment or local `.env`.
- Application thresholds from the README are exposed through validated `ExpenseRules` settings.
- `AllowedHosts` is kept permissive for the local Aspire workflow and should be restricted per deployment environment.
- PostgreSQL now uses an Aspire data volume so local demo data survive restarts
- Development seed remains idempotent.
- Backend validation enforces French postal codes as exactly 5 digits for user addresses and expense billing addresses.

--------

Frontend: Implementation choices

Explicit limitations:
- Frontend scope focuses on primary user-facing flows.
- Full frontend CRUD parity is intentionally out of scope.
- Angular uses Reactive Forms.
- Templates use modern `@if` and `@for` control flow.
- RxJS handles loading, errors and refresh flows.
- Tests use the existing Nx/Vitest toolchain.
- Tests stay focused on delivered behavior.
- User update is excluded to prioritize reports and expenses.
- User activation and deactivation are excluded from the current frontend scope.
- User deletion is excluded to avoid low-value CRUD screens.
- Expense update is excluded to keep the expense form focused.
- Expense deletion is excluded; report deletion still covers confirmation UX.
- Authentication and authorization are excluded because the README does not request them.
- Omitted frontend flows remain available in the backend.
- Backend user update, user activation/deactivation, user deletion, expense update, and individual expense deletion are intentionally not exposed in the delivered frontend workflow.

Key technical points:
- Frontend with page components for orchestration and UI components for forms and tables.
- Backend business error codes mapped to localized UI messages.
- Angular proxy supports Aspire WebApi endpoints.
- Angular signals used for local UI state and component inputs.
- RxJS dedicated to API and refresh flows.
- Frontend tests run from `frontend` with `npm test -- --run`, which delegates to Nx/Vitest.
- Keeps existing project versions: Angular 20.3.x, Nx 21.6.4, TypeScript 5.9.x and RxJS 7.8.x.
- The Angular frontend uses Reactive Forms exclusively, with focused form components and no template-driven forms.
- Frontend business validation is split deliberately: ergonomic field validation in Angular, authoritative invariants in the backend.
- RxJS streams are used for page loading, API error states and list refreshes after mutations.
- Expense creation with ISO date-only API exchange and displays business dates with French formatting as required.
- Frontend forms apply lightweight format validation for French postal codes and keep backend business rules.
- Frontend report deletion uses an explicit confirmation because deleting a report also removes its attached expenses under the documented assumption.
- Expense deletion is intentionally not exposed as a frontend action in this project.
- Report detail wording avoids duplicating backend pagination settings in static UI text.

--------

IMPLEMENTATION HISTORY

Step 0: Backend project setup and development baseline
- Reviewed the functional requirements and identified the main specification ambiguities.
- Added `.gitattributes` to normalize text files to LF line endings (as specified by existing files).
- Updated `.gitignore` for .NET, Angular/Nx, logs, coverage and generated artifacts.
- Added versionable `.vscode` workspace settings and tasks for build, Aspire debugging and frontend commands.

Step 1: Backend hexagonal structure and test baseline
- Installed .NET 9 SDK and ASP.NET Core runtime locally.
- Removed the temporary test roll-forward setting after aligning the local environment with the prerequisites.
- Added the approved backend dependencies for FluentValidation and Swagger UI.
- Reorganized the backend into initial `Core`, `Domain`, `Application`, `Infrastructure` and `Presentation` layers.
- Moved the existing user endpoint and user entity into the new layered structure without changing behavior.
- Added a single backend test project organized by layer and functional context.
- Added initial architecture and dependency-injection guard tests.
- Verified the backend with `dotnet build backend\ExpenseManagement.slnx` and `dotnet test backend\ExpenseManagement.slnx`.

Step 2a: Backend users domain and application foundation
- Implemented the `Users` context in the domain and application layers.
- Added value objects for person names, postal addresses and monthly expense quotas.
- Added user activation, deactivation, logical deletion and assignability rules.
- Added FluentValidation command validation and application services for user commands and queries.
- Added domain and application tests for the delivered user behavior.

Step 2b: Backend users persistence, API and runtime validation
- Added EF Core mapping, repository, unit of work, design-time DbContext factory and migration support.
- Added stable API error mapping and Swagger-tagged user endpoints.
- Kept Microsoft.AspNetCore.OpenApi and Swagger UI together by aligning Swashbuckle.AspNetCore to a compatible 6.9.0 version.
- Added persistence, HTTP and OpenAPI runtime regression tests for the delivered user functionality.
- Verified the feature through Aspire with PostgreSQL, migrations, Swagger UI, validation errors and user creation.

Step 3a: Backend expense reports model and persistence foundation
- Added the `ExpenseReports` domain model with a dedicated calendar month value object to avoid timezone ambiguity.
- Added the computed, non-editable report title in the expected `Full Name - Month Year` format.
- Added the expense report persistence port needed to keep EF Core behind the application boundary.
- Added EF Core mapping, repository, migration and seed data for expense reports.
- Added database indexes, including the unique `(user_id, year, month)` constraint matching the business rule.
- Added domain and persistence tests for the delivered expense report model and repository behavior.
- Kept the assumption that one expense report is allowed per user and calendar month.

Step 3b: Backend expense reports application workflow and API
- Added application validation and use cases for listing, reading and creating expense reports.
- Enforced that inactive or deleted users cannot be assigned to newly created expense reports.
- Kept the assumption that user inactivity blocks new report creation, not later edits of existing reports.
- Added Swagger-tagged HTTP endpoints for listing, reading and creating expense reports.
- Added application and HTTP tests for the delivered expense report workflow.
- Verified the feature through Aspire with PostgreSQL, migrations, OpenAPI, Swagger UI and business errors.
- Fixed local Aspire HTTPS redirection by keeping `UseHttpsRedirection` disabled in development only.
- Kept the product rule about expense report deletion documented for a later step: deleting a report is assumed to physically delete its attached expenses, while individually deleted expenses remain logically deleted.

Step 4a: Backend expense entries domain model
- Added the `ExpenseEntries` domain model with `ExpenseEntry` attached to an expense report.
- Added value objects for billing address, description, amount and currency.
- Kept `Money` intentionally simple because the README only requires positive EUR amounts.
- Limited `Currency` to EUR because no multi-currency behavior is required by the functional requirements.
- Modeled expense dates as `DateOnly` calendar values without timezone semantics.
- Enforced that an expense date must belong to its report month.
- Rejected updates that would move an expense outside its report month.
- Added logical deletion state and UTC audit timestamps for expense entries.
- Added domain tests for nominal cases, limits and business errors delivered in this step.

Step 4b: Backend expense entries persistence
- Added the expense entry persistence port and EF Core repository.
- Added EF Core mapping for expense entries, value objects, `DateOnly`, soft delete and UTC audit fields.
- Added PostgreSQL checks for positive amount, EUR currency, valid report month and date-in-report-month consistency.
- Added the `AddExpenseEntries` migration with composite FK (report-period), cascade deletion and report-scoped indexes.
- Kept cascade deletion aligned with the documented report deletion assumption: deleting a report physically deletes its entries.
- Added idempotent seed expense entries, including one logically deleted entry for later filtering checks.
- Added repository tests for active listing, ordering, soft-delete filtering, lookup and value object persistence.

Configuration cleanup: Backend app settings and EF design-time connection
- Added `.env.example` for EF Core design-time database connection settings.
- Kept `.env` ignored while allowing `.env.example` to be committed.
- Removed the hardcoded design-time database password fallback from the EF Core DbContext factory.
- Added validated `ExpenseRules` appsettings for README-driven thresholds and supported currency.
- Kept domain invariants in code while exposing simple thresholds for application workflows.
- Added tests for expense rule option defaults, validation and design-time connection string resolution.

Step 4c: Backend expense entries application workflow and API
- Added application use cases for adding, updating and logically deleting expense entries.
- Added FluentValidation commands for expense entry input validation, including date, amount, description and billing address fields.
- Added Swagger-tagged HTTP endpoints with dedicated DTOs and ISO `yyyy-MM-dd` expense date exchange.
- Kept quota enforcement, active expense listing and pagination for the dedicated next step.
- Preserved the rule that updates cannot move an expense outside its original report month.
- Added application and HTTP tests for nominal cases, validation, not-found errors, date format and logical deletion.
- Fixed a runtime seed issue caused by sharing the static EUR currency value object across EF-owned money instances.
- Fixed application logger registration so expense-entry logs no longer use the user command service category.

Step 5a: Backend monthly expense quota
- Added monthly quota enforcement when adding an expense entry.
- Counted active expense entries on the report matching the assigned user and calendar month.
- Excluded logically deleted expense entries from quota calculation.
- Returned a stable `expense_entry.monthly_quota_reached` conflict when the monthly quota is reached.
- Added application, repository and HTTP tests for the delivered quota behavior.
- Added an explicit transaction boundary and PostgreSQL row lock on the expense report to avoid concurrent quota overshoot.

Step 5b: Backend active expense listing and pagination
- Added active expense entry listing for one expense report.
- Returned expenses through the shared paged response contract.
- Applied the configured functional requirement of a page size with 5 entries.
- Excluded logically deleted expense entries from paged lists and total counts.
- Added repository, application and HTTP tests for the delivered pagination behavior.

Step 6: Backend expense report deletion
- Added physical expense report deletion with attached expense entries.
- Kept the product rule explicit in code: deleting a report physically removes its attached expenses.
- Added application, repository and HTTP tests for report deletion and not-found behavior.
- Documented that production rules should clarify archival, audit and legal retention requirements.

Backend finalize demo data and review support
- Added seed demo data for Marc Assin with quota 8 and six active expenses to exercise pagination without violating quota.
- Moved development seed data to a dedicated infrastructure seeder so `AppDbContext` only carries EF mapping concerns.
- Kept `WebApi.http` as listing-only manual request support for users, reports and paged entries.

--------

Front 0: Angular shell and routing baseline
- Replaced the Nx starter screen with a clean Angular application shell.
- Added frontend structure with `core`, `shared` and feature folders.
- Added primary routes for users, reports and report detail.
- Added shared UI placeholders for the first primary screens.

Front 1: API foundation and baseline tests
- Added frontend configuration for API base path and UI constants.
- Added API error mapping for backend and transport errors.
- Added TypeScript models and data-access services for the delivered frontend scope.
- Added baseline tests for shell, routing, configuration, error mapping and data access

Front 2a: User listing and creation flow
- Implemented user listing and user creation with Reactive Forms.
- Split user form and table into focused UI components.
- Used RxJS refresh streams for user loading, creation and API error states.
- Added Vitest coverage for the user form, user page and refresh behavior.
- Kept user update, activation, deactivation and deletion outside the current frontend scope.
- Verified with Nx test, lint, build, Angular checks and Aspire calls.

Front 2b: Expense report listing and creation
- Implemented expense report listing and creation for assignable users.
- Split expense report form and table into focused UI components.
- Used RxJS refresh streams for report loading, creation and API error states.
- Displayed duplicate-report conflicts and API errors clearly.
- Added Vitest coverage for the report form, report page and conflict display.
- Fixed report table overflow, report form button design, localized texts.
- Verified with Nx test, lint, build, Angular checks and Aspire calls.

Front 3a: Report detail and paged expense list
- Implemented report detail loading with report title, assigned user and period.
- Displayed active expense entries by pages of 5 entries.
- Added pagination controls and empty, loading and API-error states.
- Displayed expense dates in French while keeping API exchange dates as ISO date-only values.
- Split expense entry rendering into a focused UI table component.
- Added Vitest coverage for report detail loading, list rendering and pagination.
- Verified the seeded Marc Assin through Aspire (for checking the pagination requirement).

Front 3b: Expense creation form and business errors
- Implement expense creation with Reactive Forms.
- Validate required fields, amount, description length and billing address fields in the UI.
- Keep the backend as the source of truth for quota and date-in-report-month rules.
- Display quota reached, date-outside-month and validation errors returned by the backend.
- Refresh the paged expense list after successful creation.
- Add Vitest coverage for the expense form, refresh flow and business error display.
- Expense creation validates also input locally.

Backend: Enforced French postal code validation for user and expense billing addresses after frontend review exposed this hole.

Front 3c: Report deletion confirmation
- Implemented physical expense report deletion from the frontend scope.
- Showed an explicit confirmation before deletion.
- Stated that attached expenses will be permanently removed under the documented product rule.
- Navigated back to the reports list after successful deletion.
- Added Vitest coverage for the confirmation, cancellation, deletion and error flows.
