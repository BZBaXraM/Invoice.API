# Invoice.API

ASP.NET Core 10 Web API for managing invoices, customers, and reporting for an authenticated business user, plus an Angular 21 SPA client (`invoice-app/`). Built as a 4-layer clean-architecture solution with custom JWT authentication (no ASP.NET Core Identity). Runs standalone or as a full stack via Docker Compose.

## What it does

- Registration/login with email confirmation codes, password reset, refresh tokens, and logout with access-token blacklisting.
- Customer CRUD with soft delete (archive/unarchive) and hard delete (only allowed if the customer has never had an invoice sent to them).
- Invoice CRUD with rows, automatic sum calculation, status transitions, archiving, hard delete (only while `Status == Created`), and PDF export (QuestPDF). Amounts are billed in AZN (single-currency; not a stored field).
- Reports: per-customer, per-service, and per-status invoice statistics over a date range.
- Realtime updates over SignalR (`/hubs/notifications`) — the client's dashboard/lists refresh live as customers/invoices change.
- Multi-language UI (Russian/English/Azerbaijani) — translation dictionaries are served by the API and fetched by the Angular client; see "Localization" below.
- All data is strictly scoped to the owning user (`ownerUserId` from the JWT) — a user only ever sees their own records.

## Architecture

Solution file: `Invoice.slnx`. Dependency direction (via `ProjectReference` only):

```
Invoice.Domain  ←  Invoice.Application  ←  Invoice.Infrastructure  ←  Invoice.API
```

- **Invoice.Domain** — entities (`User`, `Customer`, `Invoice`, `InvoiceRow`, `BaseEntity`), `Enums/InvoiceStatus`, the result wrapper `Common/ResponseModel`, `Exceptions/ApiException`. No ASP.NET Core/EF Core references.
- **Invoice.Application** — DTOs (requests/responses), `ServiceContracts` + `Services` (business logic: `AccountService`, `CustomerService`, `InvoiceService`, `ReportService`), `RepositoryContracts` (`IUnitOfWork` + one interface per aggregate), `Validators` (FluentValidation), `Extensions` (manual `To*Response()` mapping), `Helpers/PasswordHelper` (BCrypt wrapper), `DependencyInjection.AddApplication()`. Also has no ASP.NET Core/EF Core references.
- **Invoice.Infrastructure** — `Data/InvoiceDbContext`, `Repositories/` (repo implementations + `UnitOfWork`), `Services/` (`JwtService`, `CurrentUserService`, `EmailService`, `BlackListService`, `TranslationService`), `Realtime/` (`NotificationsHub`, `SignalRRealtimeNotifier`), `Migrations/`, `DependencyInjection.AddInfrastructure()`. Has a `FrameworkReference` to `Microsoft.AspNetCore.App` (needed for `IHttpContextAccessor`/JwtBearer).
- **Invoice.API** — `Controllers/` (`AccountController`, `CustomerController`, `InvoiceController`, `ReportsController`, `TranslationController`), `Resources/Localization/{ru,eng,az}.json`, `Middlewares/` (`ExceptionMiddleware`, `BlackListMiddleware`), `Program.cs` composition root.
- **invoice-app** — Angular 21 SPA client (standalone components + signals, Bun package manager). See "Frontend" below.

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
- Currency is fixed at `"AZN"`: `InvoiceResponse`/`ServiceStatResponse`/`CustomerStatResponse` each expose a `Currency` property defaulting to that constant (not persisted anywhere), and the PDF export prints `AZN` next to every amount.
- PDF export (`InvoiceService.ExportToPdfAsync`) renders via QuestPDF (Community license) and returns raw `byte[]` — no on-disk caching under `wwwroot`.
- Reports (`ReportService`/`ReportRepository`) filter on `Invoice.StartDate >= from && Invoice.EndDate <= to`, exclude soft-deleted invoices/customers, and are always scoped to the authenticated user.

### Validation

FluentValidation validators are invoked manually inside each service method (`await validator.ValidateAsync(request)`) — there is **no** `AddFluentValidationAutoValidation()` middleware wired up, so a validator added to `DependencyInjection.AddApplication()` does nothing unless the owning service actually calls it.

### Localization

UI translations are served by the API, not baked into the client. `src/Invoice.API/Resources/Localization/{ru,eng,az}.json` are flat `key: string` dictionaries (dot-separated keys, e.g. `invoiceStatus.paid`); `TranslationService` (Infrastructure, singleton) loads all three at startup and falls back unsupported/missing `lang` values to Russian. The `TranslationController` endpoint (`GET /api/translations?lang=ru|eng|az`) is anonymous — the client's login/register/etc. pages need translated text before a JWT exists. On the client, `LocalizationService` fetches and caches the active dictionary and a `translate` pipe renders `{{ 'some.key' | translate }}`, with `{{param}}` interpolation support for dynamic values (names, counts). Adding a new UI string is just adding the key to all three JSON files — no code changes needed.

### Realtime updates (SignalR)

`CustomerService`/`InvoiceService` notify `IRealtimeNotifier` after every mutation (create/update/archive/unarchive/delete/status-change); `SignalRRealtimeNotifier` broadcasts to a per-user SignalR group so a logged-in user's other open tabs/sessions see changes live. Hub endpoint: `/hubs/notifications` (`[Authorize]`) — since browsers can't set an `Authorization` header on the WebSocket handshake, the client passes the JWT as an `access_token` query parameter instead.

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
| GET | `/profile` | ✓ | Get the current user's profile |
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
| GET | `/` | Paginated/filtered/sorted customer list (`pageNumber`, `pageSize`, `nameFilter`, `sortBy`, `sortDescending`, `includeArchived`) |
| DELETE | `/{id}` | Hard delete (only if the customer has never had an invoice sent) |
| POST | `/{id}/archive` | Archive (soft delete) |
| POST | `/{id}/unarchive` | Restore a previously archived customer |

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

### `TranslationController` — `api/translations`

| Method | Path | Auth | Description |
|---|---|---|---|
| GET | `/?lang=ru\|eng\|az` | — | Get the UI translation dictionary for a language (falls back to `ru` if omitted/unsupported) |

### Realtime hub

`/hubs/notifications` (`[Authorize]`, JWT passed as an `access_token` query parameter) — SignalR hub broadcasting customer/invoice create/update/archive/unarchive/delete and invoice status-change events to the owning user's connections.

## Getting started

### Option A: Docker Compose (fastest)

One-time setup — the API needs an HTTPS dev cert mounted from the host:

```bash
dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p 123 --trust
```

```bash
docker compose up --build
```

Brings up Postgres (`localhost:5432`), the API (`localhost:7030` HTTPS / `localhost:6000` HTTP), and the client behind nginx (`localhost:4200`) — migrations auto-apply on first boot. The client and API are separate origins: the browser talks directly to the API's HTTPS port rather than going through an nginx proxy, so CORS (`Cors:AllowedOrigins`) has to allow the client's origin. `docker compose down -v` tears everything down, including the Postgres volume.

### Option B: Run locally

#### Requirements

- .NET 10 SDK
- PostgreSQL listening on `localhost:5434` (see `ConnectionStrings:DefaultConnection` in `src/Invoice.API/appsettings.json` — the committed default connects on port `5432`; `5434` reflects a locally-remapped install)
- SMTP access (Gmail by default) for sending confirmation/reset emails — fill in `EmailConfig` in `appsettings.json`/`appsettings.Development.json`
- Bun (for `invoice-app/`, if running the frontend too)

#### Before the first run

In `src/Invoice.API/appsettings.json` (or `appsettings.Development.json`), set:

- `ConnectionStrings:DefaultConnection` — your Postgres connection string.
- `JWT:Secret` — a random string **at least 64 characters** long (see the `HmacSha512Signature` gotcha above).
- `EmailConfig:From`/`UserName`/`Password` — real mailbox credentials (for Gmail, an App Password, not the regular account password).
- `Cors:AllowedOrigins` — must include whatever origin the client runs on (defaults to `https://localhost:4200`, matching `bun run start` below).

#### Backend

```bash
dotnet build Invoice.slnx
dotnet run --project src/Invoice.API
dotnet ef migrations add <Name> --project src/Invoice.Infrastructure --startup-project src/Invoice.API
dotnet ef database update --project src/Invoice.Infrastructure --startup-project src/Invoice.API
```

In Development, `Program.cs` auto-applies pending migrations on startup via `InitialiseDatabaseAsync()` — running `database update` manually isn't required for local development.

Once running, Swagger UI is available at the app's URL (Development only), e.g. `https://localhost:7030/swagger`.

No test projects exist yet.

#### Frontend

```bash
cd invoice-app
bun install
bun run start   # ng serve over HTTPS at https://localhost:4200 (see .cert/)
```

`environment.development.ts` points at `https://localhost:7030/api` — run the backend first. `bun run build` produces the production bundle (used by the Docker image); it targets the same absolute `https://localhost:7030/api` (`environment.ts`) and hits the API directly rather than going through nginx.
