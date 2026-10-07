# Multi-Tenant Employee API

REST API for managing employees of multiple tenants. Built with .NET 8, EF Core, PostgreSQL, MediatR and FluentValidation.

## How to run

```bash
docker compose up --build
```

This starts PostgreSQL and the API. Migrations and the two seed tenants are applied automatically on startup.

- Swagger: http://localhost:8080/swagger
- Example requests: `src/MultiTenantEmployeeApi/MultiTenantEmployeeApi.http`

Every `/api` request needs the `X-Tenant-Id` header. Seeded tenants:

| Tenant   | Id                                     |
|----------|----------------------------------------|
| Tenant A | `11111111-1111-1111-1111-111111111111` |
| Tenant B | `22222222-2222-2222-2222-222222222222` |

## Running tests

```bash
dotnet test
```

Docker must be running, because the integration tests start a real PostgreSQL container using Testcontainers.

## Endpoints

| Method | Route                     | Description                            |
|--------|---------------------------|----------------------------------------|
| POST   | `/api/v1/employees`       | Create employee                        |
| GET    | `/api/v1/employees`       | List (paging + filtering)              |
| GET    | `/api/v1/employees/{id}`  | Get by id                              |
| PUT    | `/api/v1/employees/{id}`  | Update                                 |
| DELETE | `/api/v1/employees/{id}`  | Soft delete                            |

List query parameters: `page` (default 1), `pageSize` (default 20, max 100), `department`, `status` (`Active`/`Suspended`), `search` (first name, last name or email).

All responses use the same envelope:

```json
{
  "data": { },
  "pagination": { "page": 1, "pageSize": 20, "totalCount": 42, "totalPages": 3 },
  "error": null
}
```

On error `data` is null and `error` has a `code`, `message` and optional `details` (validation errors per field).

| Status | When                                      |
|--------|-------------------------------------------|
| 400    | Validation error / missing or invalid tenant header |
| 403    | Tenant id does not exist                  |
| 404    | Employee not found (or belongs to another tenant) |
| 409    | Email already used in the same tenant     |

## Project structure

```
src/MultiTenantEmployeeApi
  Controllers/        thin controllers, they only send commands/queries to MediatR
  Features/Employees/
    Commands/         Create, Update, Delete (command + handler + validator)
    Queries/          GetEmployees, GetEmployeeById
    Dtos/
  Entities/           Employee, Tenant, TaskItem, Money
  Data/               DbContext, EF configurations, migrations
  Tenancy/            tenant middleware + tenant provider
  Common/             response envelope, exceptions, validation behavior, error middleware
tests/MultiTenantEmployeeApi.Tests
  Unit/               handler and validator tests
  Integration/        API tests against real PostgreSQL (Testcontainers)
```

## Design decisions

### Multi-tenancy

- `TenantMiddleware` reads `X-Tenant-Id`, checks it is a valid UUID and that the tenant exists in the `Tenants` table, then stores it in a scoped `ITenantProvider`.
- `AppDbContext` has a **global query filter** on `Employee`: `TenantId == CurrentTenantId && DeletedAt == null`. So every query (list, get, update, delete) is automatically limited to the current tenant, and handlers can't forget to add the filter.
- `TenantId` is set in `SaveChangesAsync` from the tenant provider, it is never taken from the request body.
- Accessing an employee of another tenant returns **404** (not 403), so the API doesn't reveal that the id exists.
- Email is unique per tenant (unique index on `TenantId + Email`, only for not deleted rows). The same email can exist in two tenants.

### CQRS

Writes are commands, reads are queries, all go through MediatR. A `ValidationBehavior` pipeline runs the FluentValidation validators before any handler, so handlers only contain business logic. Read handlers use `AsNoTracking`.

### Error handling

Handlers throw `NotFoundException` / `ConflictException` / `ValidationException` and `ExceptionHandlingMiddleware` maps them to the status code and the response envelope. Model binding errors (bad JSON, unknown enum value) are also returned in the same envelope. Unexpected errors are logged and return a generic 500 message.

### Database

- Naming is PascalCase for all tables and columns (`Employees`, `FirstName`, `TenantId`, ...).
- `CustomData` is a `jsonb` column. The API accepts any JSON object.
- `Status` is stored as a string (`Active` / `Suspended`) to keep the data readable.
- Soft delete: `DELETE` only sets `DeletedAt`, the query filter hides the row.
- `CreatedAt` / `UpdatedAt` are set automatically in `SaveChangesAsync` (UTC).
- `Tasks` table (`Id`, `Name`, `Description`, `DueDate`) is created for later use. The entity is called `TaskItem` to avoid conflict with `System.Threading.Tasks.Task`.

### Salary (Money)

Salary is a `Money` value object stored as `SalaryAmountMinor` (bigint, amount in cents) and `SalaryCurrencyCode` (ISO 4217, 3 letters). No float/decimal is used for money.

### Tests

- Unit tests (`Unit/`): create handler (happy path, duplicate email, same email in another tenant), list handler (pagination, filtering), tenant isolation (list, get, update, delete, soft delete) and validator.
  They use the EF InMemory provider only to test handler logic fast.
- Integration tests (`Integration/`): run the whole API with `WebApplicationFactory` against a **real PostgreSQL** started by Testcontainers. They check tenant isolation end to end, the envelope, 400/403/404/409 responses and soft delete.

### Packages

- MediatR is pinned to 12.x because newer versions require a commercial license.

## What I would add with more time

- Custom field definitions per tenant to validate `CustomData`
- PostgreSQL Row-Level Security as a second layer of isolation
- Audit log table (who changed what and when)
- Real authentication where the tenant comes from the token instead of a plain header
