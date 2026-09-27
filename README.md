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

Project Status

The project is currently under active development. The backend is being developed first, with the frontend planned as the next stage.
