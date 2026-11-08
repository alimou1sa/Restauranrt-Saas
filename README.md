RestaurantSaas_Api

A multi-tenant SaaS backend designed to help restaurants manage their operations through a centralized system.

Features

- Multi-tenant architecture
- Organization and branch management
- User authentication and authorization
- Role-Based Access Control (RBAC)
- Custom roles and permissions
- Branch-level access control
- Menu, category, and product management
- Inventory management
- Order and order-item management
- JWT-based authentication
- DTO-based API design

Architecture

The project follows Clean Architecture and is organized into four main layers:

RestaurantSaas.Api
        ↓
RestaurantSaas.Application
        ↓
RestaurantSaas.Infrastructure
        ↓
RestaurantSaas.Domain

- Domain — Entities, enums, and core business concepts.
- Application — DTOs, services, interfaces, and application logic.
- Infrastructure — Entity Framework Core, SQL Server, authentication, configurations, and external services.
- API — Controllers, middleware, dependency injection, and HTTP endpoints.

Technologies

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- LINQ
- JWT Authentication
- REST API
- Swagger / OpenAPI
- Git & GitHub

Run with Docker

Everything you need to run the API and SQL Server locally:

```bash
docker compose up --build
```

- API: http://localhost:8080 (Swagger UI at http://localhost:8080/swagger)
- SQL Server: localhost:1433 (user `sa`, password from `MSSQL_SA_PASSWORD`, default `YourStrong!Passw0rd`)

The container creates the database schema on first start (`Database:AutoCreate=true`).
To use your own SQL Server instance instead, change `ConnectionStrings__DefaultConnection` in
`docker-compose.yml` and set `Database__AutoCreate` to `false`.

Override secrets without editing the file:

```bash
MSSQL_SA_PASSWORD='MyStr0ng!Pass' JWT_KEY='a-real-32-char-secret-key' docker compose up --build
```

Tests

```bash
dotnet test
```

28 xUnit tests cover order rules, payments, password hashing, refresh tokens, and tenant isolation.

Project Status

The project is currently under active development. The backend is being developed first, with the frontend planned as the next stage.
