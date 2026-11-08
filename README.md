# Restaurant SaaS - Multi-Tenant Backend API

A multi-tenant SaaS backend for restaurant management: organizations and branches, menus and
products, orders, payments, inventory, subscriptions, and permission-based access control -
built with ASP.NET Core 8, EF Core and SQL Server.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-compose-2496ED?logo=docker&logoColor=white)
![xUnit](https://img.shields.io/badge/tests-28%20passing-brightgreen)

---

## Overview

Restaurant SaaS is the backend of a platform where one deployment serves many restaurant
businesses. Each organization signs up, creates branches, invites users, and manages its own
menu, orders and inventory - without ever seeing another organization's data.

The API exposes **113 endpoints across 26 controllers** over **23 entities**, and is built to be
read by a future React/TypeScript dashboard.

What makes it more than a CRUD project:

- **Tenant isolation is enforced at the data layer**, not in controllers (see below).
- **Authorization is permission-based and dynamic**: roles and their permissions are rows in the
  database, so a new permission does not require new code or a redeploy.
- **Platform administration is separated from restaurant-level management** by token type, so a
  platform admin token can never be used on organization endpoints and vice versa.

## Architecture

Clean Architecture with four layers and a strict dependency direction:

```
RestaurantSaas_Api            controllers, middleware, DI wiring, Swagger
        |
        v
RestaurantSaaS.Application    DTOs, service interfaces and business services
        |
        v
RestaurantSaaS.Infrastructure EF Core, AppDbContext, auth, password/refresh-token hashing
        |
        v
RestaurantSaaS.Domain         entities, enums, shared interfaces (ITenantEntity, ICurrentTenant)
```

| Layer | Responsibility |
| --- | --- |
| `RestaurantSaaS.Domain` | 23 entities, enums, marker interfaces - no dependencies |
| `RestaurantSaaS.Application` | 28 services, DTOs, interfaces; rules live here (e.g. `OrderStatusRules`, `PaymentRules`) |
| `RestaurantSaaS.Infrastructure` | `AppDbContext`, entity configurations, JWT, RBAC handlers, BCrypt/SHA-256 hashers |
| `RestaurantSaas_Api` | 26 controllers (22 organization + 4 platform), Swagger/OpenAPI |

### How tenant isolation works

Every tenant-owned entity implements `ITenantEntity`. `AppDbContext` scans the model at startup
and attaches a global query filter to each one, so **every** LINQ query is scoped automatically:

```csharp
// Simplified: the filter is built once per entity type at model creation
modelBuilder.Entity<Order>().HasQueryFilter(e => e.OrganizationId == CurrentOrganizationId);
```

`CurrentOrganizationId` comes from `ICurrentTenant`, which reads the claim on the current JWT.
When there is no organization context the value is `null`, and the filter becomes
`OrganizationId = NULL` - which returns **zero rows** instead of throwing or leaking.

Branch-scoped entities are filtered the same way, one level deeper. The test suite asserts that
a query executed by one tenant can never return another tenant's rows.

### How authorization works

Authentication is JWT access tokens (15 minutes) plus rotating refresh tokens (30 days). Refresh
tokens are stored hashed with SHA-256; passwords with BCrypt. Revoked access tokens are added to
an in-memory blacklist that is checked on every request.

Authorization uses a custom policy provider instead of hard-coded roles:

1. A controller action is decorated with `[HasPermission("Orders.Create")]`.
2. `PermissionPolicyProvider` creates a policy on demand for that permission string.
3. `PermissionAuthorizationHandler` checks the permission claim issued in the access token.

Because permissions are database rows assigned to roles, an organization can define **custom
roles** with any combination of permissions, and the API enforces them without changes.

## Features

| Module | Capabilities |
| --- | --- |
| Organizations & branches | Create organizations, manage branches, branch-level data scoping |
| Users & roles | Organization users, roles, role-permissions, system roles, user-role assignment |
| Auth | Login, refresh tokens, organization selection, logout/blacklist, profile |
| Menu | Menus, categories, products with pricing |
| Orders | Orders, order items, status rules (no negative stock under concurrency) |
| Payments | Payments with validation rules per payment method |
| Inventory | Stock items and inventory transactions |
| Subscriptions | Plans, subscriptions per organization, payment linkage |
| Platform admin | Platform organizations, users, plans, subscriptions, admin impersonation |

## Project structure

```
RestaurantSaaS/
├── RestaurantSaaS.Domain/          entities and shared abstractions
├── RestaurantSaaS.Application/     services, DTOs, interfaces, business rules
├── RestaurantSaaS.Infrastructure/  EF Core, AppDbContext, auth, hashers
├── RestaurantSaas_Api/             controllers, Program.cs, Swagger, appsettings
├── RestaurantSaaS.Tests/           28 xUnit tests
├── Dockerfile                      multi-stage build
└── docker-compose.yml              API + SQL Server 2022
```

## Run with Docker

Everything you need - API and SQL Server - with one command:

```bash
docker compose up --build
```

- API base URL: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- SQL Server: `localhost:1433` (user `sa`, password from `MSSQL_SA_PASSWORD`, default
  `YourStrong!Passw0rd`)

The container creates the schema on first start (`Database:AutoCreate=true`). Data is kept in a
named volume, so it survives `docker compose down`.

Override the defaults without editing files:

```bash
MSSQL_SA_PASSWORD='MyStr0ng!Pass' JWT_KEY='a-real-32-char-secret-key' docker compose up --build
```

## Run locally

1. Create the database on a local SQL Server instance (or start only the SQL container:
   `docker compose up -d sqlserver`).
2. Set the connection string in `RestaurantSaas_Api/appsettings.json` (or user secrets).
3. Run:

```bash
dotnet run --project RestaurantSaas_Api
```

Swagger is available in the Development environment at `/swagger`.

## Tests

```bash
dotnet test
```

```
Passed! - Failed: 0, Passed: 28, Skipped: 0, Total: 28
```

The suite covers the parts that are easiest to get wrong:

- tenant isolation (a tenant cannot read another tenant's rows)
- order rules and the concurrency guard against negative stock
- payment rules and payment service behaviour
- password hashing (BCrypt) and refresh-token hashing
- tenant filter behaviour with and without an organization context

Tests run against a real throwaway LocalDB database, so they exercise the actual EF Core query
filters rather than mocks.

## Configuration

| Key | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Key` | Signing key (32+ chars) |
| `Jwt__Issuer` / `Jwt__Audience` | Token validation |
| `Jwt__ExpirationMinutes` / `Jwt__RefreshTokenExpirationDays` | Token lifetimes |
| `Database__AutoCreate` | Create the schema on startup (used by docker-compose) |

Never commit a real JWT key; `appsettings.json` ships a placeholder on purpose.

## Notes and limitations

- There are no EF migrations yet, so Docker relies on `EnsureCreated`. Adding migrations is the
  next step.
- The access-token blacklist is in memory, which means it is per-instance; a Redis-backed
  implementation would be needed to scale out.
- The React/TypeScript dashboard is not included in this repository yet - the backend was built
  first.

## Roadmap

- EF Core migrations + CI pipeline (build, test, publish image)
- Redis-backed token blacklist
- Rate limiting and audit logging
- React + TypeScript dashboard
