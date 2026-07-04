# Invoice.API

ASP.NET Core 10 Web API for managing invoices, customers, and reporting for an authenticated business user. Built as a 4-layer clean-architecture solution with custom JWT authentication (no ASP.NET Core Identity).

## What it does

- Registration/login with email confirmation codes, password reset, refresh tokens, and logout with access-token blacklisting.
- Customer CRUD with soft delete (archive) and hard delete (only allowed if the customer has never had an invoice sent to them).
- Invoice CRUD with rows, automatic sum calculation, status transitions, archiving, hard delete (only while `Status == Created`), and PDF export (QuestPDF).
- Reports: per-customer, per-service, and per-status invoice statistics over a date range.
- All data is strictly scoped to the owning user (`ownerUserId` from the JWT) — a user only ever sees their own records.

## Architecture

Solution file: `Invoice.slnx`. Dependency direction (via `ProjectReference` only):

```
Invoice.Domain  ←  Invoice.Application  ←  Invoice.Infrastructure  ←  Invoice.API
```

- **Invoice.Domain** — entities (`User`, `Customer`, `Invoice`, `InvoiceRow`, `BaseEntity`), `Enums/InvoiceStatus`, the result wrapper `Common/ResponseModel`, `Exceptions/ApiException`. No ASP.NET Core/EF Core references.
- **Invoice.Application** — DTOs (requests/responses), `ServiceContracts` + `Services` (business logic: `AccountService`, `CustomerService`, `InvoiceService`, `ReportService`), `RepositoryContracts` (`IUnitOfWork` + one interface per aggregate), `Validators` (FluentValidation), `Extensions` (manual `To*Response()` mapping), `Helpers/PasswordHelper` (BCrypt wrapper), `DependencyInjection.AddApplication()`. Also has no ASP.NET Core/EF Core references.
- **Invoice.Infrastructure** — `Data/InvoiceDbContext`, `Repositories/` (repo implementations + `UnitOfWork`), `Services/` (`JwtService`, `CurrentUserService`, `EmailService`, `BlackListService`), `Migrations/`, `DependencyInjection.AddInfrastructure()`. Has a `FrameworkReference` to `Microsoft.AspNetCore.App` (needed for `IHttpContextAccessor`/JwtBearer).
- **Invoice.API** — `Controllers/` (`AccountController`, `CustomerController`, `InvoiceController`, `ReportsController`), `Middlewares/` (`ExceptionMiddleware`, `BlackListMiddleware`), `Program.cs` composition root.

### Result pattern

Every service method returns `ResponseModel`/`ResponseModel<T>` (`Message`, `IsSucceeded`, `StatusCode`, `Errors`) instead of throwing for expected business failures (not-found, validation, state conflicts) — via the `ResponseModel.Success(...)`/`ResponseModel.Failure(...)` static factories. Every controller action does `return StatusCode(result.StatusCode, result);` uniformly. Only truly unexpected exceptions should reach `ExceptionMiddleware`'s 500 handler.

### Repository + UnitOfWork

No generic repository base — one bespoke interface per aggregate (`IUserRepository`, `ICustomerRepository`, `IInvoiceRepository`, `IReportRepository`). `InvoiceRow` has no repository of its own; it's owned entirely through the `Invoice` aggregate. `IUnitOfWork` exposes one lazily-instantiated property per repository plus `CommitAsync()`. Only `IUnitOfWork` is registered in DI — individual repositories are never resolved from the container directly, they come from `uow.XRepository`.

### Custom JWT auth (no ASP.NET Identity)

`User` (Domain) carries a BCrypt password hash, a nullable `RefreshToken`/`RefreshTokenExpireTime`, and an email-confirmation flag/code — there is no separate Identity user store. `JwtService` (Infrastructure) manually builds tokens via `JwtSecurityTokenHandler`/`HmacSha512Signature`; claims are `ClaimTypes.NameIdentifier` (user id) and `ClaimTypes.Name` (email). Standard `AddAuthentication().AddJwtBearer()` middleware still validates requests at runtime — only the user-store/sign-in side was replaced.

- Login/register/refresh set/rotate the stored refresh token (7-day expiry); change-password and delete-profile null it out to force re-login.
- Registration requires email confirmation via a code (`EmailService`, MailKit) — there are endpoints to resend the code.
- Logout blacklists the access token (`BlackListService`); `BlackListMiddleware` rejects requests carrying a blacklisted token with a 401 before `Authorization` runs.
- `ICurrentUserService`/`CurrentUserService` reads the current user id off `ClaimTypes.NameIdentifier` via `IHttpContextAccessor` — every `CustomerService`/`InvoiceService`/`ReportService` method takes an `ownerUserId` from this, not from a route/body parameter, so all data is scoped to the authenticated caller (`Customer`/`Invoice` both carry an explicit `UserId`).

**Gotcha for the JWT secret**: `HmacSha512Signature` requires a key of at least 512 bits (64+ chars). A short `JWT:Secret` in `appsettings.json` fails at token-signing time with an `IDX10720` error, not at startup — always use a long (64+ char) secret.

**Gotcha for `DateTimeOffset` columns**: Npgsql's `timestamp with time zone` only accepts UTC-offset `DateTimeOffset` values. `InvoiceDbContext.OnModelCreating` installs a global `ValueConverter` that normalizes every `DateTimeOffset`/`DateTimeOffset?` property to UTC on save — don't remove it, or writes with a non-UTC local offset will throw at `SaveChangesAsync()`.

### Business invariants

- Soft delete via `DeletedAt` (`Archive*` actions) plus a true hard-delete gated on state: `Invoice.UpdateAsync`/`DeleteAsync` only allow `Status == Created`; `Customer.DeleteAsync` only allowed if the customer has zero non-`Created` (i.e., ever-sent) invoices (`ICustomerRepository.HasSentInvoicesAsync`).
- `InvoiceRow.Sum = Quantity * Rate` and `Invoice.TotalSum` = sum of row sums are computed in `InvoiceService` on every create/update — not DB-computed columns.
- PDF export (`InvoiceService.ExportToPdfAsync`) renders via QuestPDF (Community license) and returns raw `byte[]` — no on-disk caching under `wwwroot`.
- Reports (`ReportService`/`ReportRepository`) filter on `Invoice.StartDate >= from && Invoice.EndDate <= to`, exclude soft-deleted invoices/customers, and are always scoped to the authenticated user.

### Validation

FluentValidation validators are invoked manually inside each service method (`await validator.ValidateAsync(request)`) — there is **no** `AddFluentValidationAutoValidation()` middleware wired up, so a validator added to `DependencyInjection.AddApplication()` does nothing unless the owning service actually calls it.

## Endpoints

Base prefix: `api/`. All `[Authorize]` endpoints require an `Authorization: Bearer <access token>` header.

### `AccountController` — `api/account`

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/register` | — | Register a new user, sends an email confirmation code |
| POST | `/confirm-email-code` | — | Confirm email using the sent code |
| POST | `/request-confirmation-code` | — | Resend the email confirmation code |
| POST | `/login` | — | Log in, returns an access/refresh token pair |
| POST | `/refresh-token` | — | Exchange a refresh token for a new token pair |
| POST | `/logout` | ✓ | Blacklists the access token and clears the refresh token |
| POST | `/forget-password` | — | Request a password reset code by email |
| POST | `/reset-password` | — | Reset password using the code sent by email |
| PUT | `/profile` | ✓ | Update the current user's profile |
| PUT | `/profile/change-password` | ✓ | Change the current user's password |
| DELETE | `/profile` | ✓ | Delete the current user's profile (hard delete) |

### `CustomerController` — `api/customers`

All endpoints `[Authorize]`.

| Method | Path | Description |
|---|---|---|
| POST | `/` | Create a customer |
| PUT | `/{id}` | Update a customer |
| GET | `/{id}` | Get a customer by id |
| GET | `/` | Paginated/filtered/sorted customer list (`pageNumber`, `pageSize`, `nameFilter`, `sortBy`, `sortDescending`) |
| DELETE | `/{id}` | Hard delete (only if the customer has never had an invoice sent) |
| POST | `/{id}/archive` | Archive (soft delete) |

### `InvoiceController` — `api/invoices`

All endpoints `[Authorize]`.

| Method | Path | Description |
|---|---|---|
| POST | `/` | Create an invoice |
| PUT | `/{id}` | Update an invoice (only while `Status == Created`) |
| PUT | `/{id}/status` | Change an invoice's status |
| GET | `/{id}` | Get an invoice by id |
| GET | `/` | Paginated/filtered/sorted invoice list (`pageNumber`, `pageSize`, `customerId`, `status`, `sortBy`, `sortDescending`) |
| DELETE | `/{id}` | Hard delete (only while `Status == Created`) |
| PUT | `/{id}/archive` | Archive (soft delete) |
| GET | `/{id}/export` | Export the invoice to PDF |

### `ReportsController` — `api/reports`

All endpoints `[Authorize]`, take `from`/`to` (`DateTimeOffset`) as query parameters.

| Method | Path | Description |
|---|---|---|
| GET | `/customers` | Per-customer invoice count and total sum for the period |
| GET | `/services` | Per-service invoice count and total sum for the period |
| GET | `/invoice-status` | Invoice counts grouped by status for the period |

## Getting started

### Requirements

- .NET 10 SDK
- PostgreSQL listening on `localhost:5434` (see `ConnectionStrings:DefaultConnection` in `src/Invoice.API/appsettings.json`)
- SMTP access (Gmail by default) for sending confirmation/reset emails — fill in `EmailConfig` in `appsettings.json`/`appsettings.Development.json`

### Before the first run

In `src/Invoice.API/appsettings.json` (or `appsettings.Development.json`), set:

- `ConnectionStrings:DefaultConnection` — your Postgres connection string.
- `JWT:Secret` — a random string **at least 64 characters** long (see the `HmacSha512Signature` gotcha above).
- `EmailConfig:From`/`UserName`/`Password` — real mailbox credentials (for Gmail, an App Password, not the regular account password).

### Commands

```bash
dotnet build Invoice.slnx
dotnet run --project src/Invoice.API
dotnet ef migrations add <Name> --project src/Invoice.Infrastructure --startup-project src/Invoice.API
dotnet ef database update --project src/Invoice.Infrastructure --startup-project src/Invoice.API
```

In Development, `Program.cs` auto-applies pending migrations on startup via `InitialiseDatabaseAsync()` — running `database update` manually isn't required for local development.

Once running, Swagger UI is available at the app's URL (Development only), e.g. `https://localhost:7030/swagger`.

No test projects exist yet.
