# Module structure 

Features are organized into **modules**. A module is a set of projects that implement a single business capability end-to-end (domain + API + UI). Each module currently consists of:

- `PaintingProjectsManagement.Features.Inventory.Core`
- `PaintingProjectsManagement.Features.Inventory.Core.Contracts`
- `PaintingProjectsManagement.Features.Inventory.Integrations` 
- `PaintingProjectsManagement.Features.Inventory.Integrations.Contracts`  
- `PaintingProjectsManagement.Features.Inventory.Web`
- `PaintingProjectsManagement.Features.Inventory.Web.Contracts`
- `PaintingProjectsManagement.Features.Inventory.UI`

## Project responsibilities

### `*.Core`
The module **core domain** layer.

Should contain:
- `Database/` – EF Core model configuration (e.g. `*Config` classes)
- `Models/Domain/` – domain entities (e.g. `PaintBrand`, `PaintLine`, `PaintColor`, ...)
- `Models/Events/` – domain events (e.g. `PaintBrandCreated`, `PaintLineUpdated`, `PaintColorDeleted`, ...)
- `Services/` – domain services used by the module
- `Usings.cs` – module-level usings

Should NOT contain:
- Web/API use-cases/endpoints
- UI components

### `*.Core.Contracts`
The module public **shared contracts** intended for other modules and presentation layers.

Should contain:
- Cross-module shared types (mostly enums and constants)

### `*.Core.Tests`
Tests for the core domain layer. All tests here should be unit tests, and not depend on any external resources (e.g. database, file system, network).

Tests should cover the domain entities and the domain services.

### `*.Web`
The module **HTTP API host** (ASP.NET Core endpoints for the module) meant to serve exclusively the UI layer.

Should contain:
- `Localization/` - localization enums to be used by the localization framework
- `DataTransfer/` - reusable DTOs returned by the API. These should be used across multiple endpoints, and are not meant to be one-off request/response models. Usually sufixed with `Details`, e.g. `PaintBrandDetails`, `CatalogDetails`
- `UseCases/` – vertical slices grouped by feature (e.g. `PaintBrands`, `PaintLines`, `PaintColors`, `MyPaints`, `Catalog`)
  - `Commands/` – write operations
  - `Queries/` – read operations
  - `Builder.cs` – per-slice registrations/route composition
- `Usings.cs` – module-level usings


### `*.Web.Contracts`
Contracts specific to the **web surface** of the module.

Use this project when you need types that are:
- Shared between `*.Web` and Blazor consumers 
- HTTP-facing and should not leak internal domain/application details

Typical contents:
- Interfaces for shared request/response shapes between API and clients. These interfaces are meant to keep consistency between the API and its consumers without exposing internal domain models directly. This allows for the backend to an implementation focused in the creation and serialization (i.e. private sets, init, required) and the frontend to have an implmentation focused in the deserialization.

Folder layout (as used today):
- `DataTransfer/` – `I*Details` interfaces (e.g. `IPaintBrandDetails`)
- `Requests/` – request interfaces (e.g. `IGetCatalogRequest`, `IAddToMyPaintsRequest`)
- `Usings.cs`

### `*.UI`
The module **Blazor UI** (WebAssembly) layer.

Should contain:
- API client/service wrappers that call `*.Web` (or the API host)
- UI-side models matching the module needs
- Module menu registration and module wiring

Folder layout (as used today):
- `Services/` (e.g. `InventoryService`)
- `Models/`
  - `Details/` (e.g. `*Details`)
  - `Requests/` (module request models, used in http calls)
- `Menu.cs` (configuration for the side menu
- `Builder.cs`

## Module naming and references

- Module projects are named: `PaintingProjectsManagement.Features.<ModuleName>.<Layer>`
- Cross-module references should prefer `*.<Layer>.Contracts` 
- `*.Web` should reference `*.Core` and the appropriate contracts projects
- `*.Integrations` should reference `*.Core` and the appropriate contracts projects
- `*.UI` should reference `*.Core.Contracts` and/or `*.Web.Contracts` (referencing `*.Core` or other layers directly is forbidden)


### `*.Integrations`

This is meant for module-to-module communication within the solution. When a module needs to consume another module's functionality, it should do so via this project using `IDispatcher` commands/queries. 

Should contain:
- `Queries/` and `Commands/` – Implementation of business capabilities using the same patterns in the `*.Web` layer (`IDispatcher` commands). But here only the `Validator` and `Handler` are nested in the main class. The request is in the `*.Integrations.Contracts` because it needs to be shared betweem layers. Commands are meant for operations that must the synchoronous and the caller must wait for the result, otherwise use the events framework.
- `Usings.cs` – module-level usings

Should NOT contain:
- Web/API use-cases/endpoints
- UI components

### `*.Integrations.Contracts`
The module public **shared contracts**, meant to the consumed by other modules from the application.

Should Contain:
- `{Operation}/` - request and result objects for the operation (e.g. `ColorMatchResult` and `FindColorMatchesQuery`)

Should contain:
- Cross-module requests (commands and queries) models

---

# Solution structure

The repository is organized into top-level folders by architectural concern:

```
src/
├── Aspire/
│   ├── AppHost/                    ← .NET Aspire orchestrator (Program.cs wires everything)
│   └── ServiceDefaults/            ← Shared Aspire service defaults (telemetry, health checks)
├── Application/
│   ├── Api/                        ← ASP.NET Core API host (Program.cs, Dockerfile)
│   └── UI/                         ← Blazor WASM host (Program.cs, App.razor)
├── Features/
│   ├── Modules/                    ← Feature modules (Inventory, Materials, Models, Projects, Subscriptions)
│   └── Shared/
│       ├── Authentication/         ← Auth + Authorization shared features
│       ├── Common/                 ← Core shared (EntityReference, EnumReference, StorageQuota)
│       ├── Currency/               ← Currency feature
│       └── Testing/                ← Shared test utilities
├── Infrastructure/
│   └── Common/                     ← Infrastructure cross-cuts
├── Persistance/
│   └── Database/                   ← EF Core DatabaseContext, MessagingDbContext, migrations
└── PaintingProjectsManagment.slnx  ← Solution file
```

- **Aspire/** — Orchestrates all services (PostgreSQL, RabbitMQ, API, UI, pgAdmin). `AppHost/Program.cs` wires everything. `ServiceDefaults/` provides shared telemetry and health check configuration.
- **Application/** — The two runtime hosts: `Api/` (ASP.NET Core backend) and `UI/` (Blazor WASM frontend).
- **Features/** — All business logic lives here as isolated modules. See the "Module structure" section for detailed project breakdown.
- **Infrastructure/** — Cross-cutting infrastructure concerns (currently minimal).
- **Persistance/** — EF Core contexts (`DatabaseContext`, `MessagingDbContext`) and migrations. The main `DatabaseContext` is instrumented with the `OutboxSaveChangesInterceptor` to write domain events atomically.

---

# How to run

## Prerequisites

Docker must be running. On Linux VMs without a Docker daemon:
```bash
sudo dockerd &>/path/to/dockerd.log &
sudo chmod 666 /var/run/docker.sock
```

## Start the application

From `src/`:
```bash
dotnet run --project Aspire/PaintingProjectsManagement.AppHost -- --allow-unsecured-transport
```

The `--allow-unsecured-transport` flag avoids HTTPS certificate issues in headless/dev environments.

## Service endpoints

| Service | URL |
|---------|-----|
| UI (HTTPS — **use this**) | `https://localhost:7114` |
| UI (HTTP — **do NOT use**) | `http://localhost:5251` |
| API | `https://localhost:7236` |
| Aspire Dashboard | `https://localhost:17257` |
| pgAdmin | `http://localhost:5050` |
| RabbitMQ Management | `http://localhost:15672` |

**Critical:** Always access the UI via HTTPS (`https://localhost:7114`). The API enforces HTTPS redirect, so the Blazor WASM client must be served over HTTPS for API calls to work through Aspire's service discovery. Using `http://localhost:5251` will cause login and all API calls to fail silently.

---

# Architecture overview

The application is a **modular monolith** orchestrated by .NET Aspire. All services run as separate processes coordinated through Aspire's service discovery and resource management.

## High-level layers

1. **Aspire AppHost** — Orchestrates PostgreSQL, RabbitMQ, the API, and the Blazor UI. Manages startup dependencies and service discovery.
2. **API Host** (`Application/Api`) — ASP.NET Core backend. Registers feature modules, wires up authentication, telemetry, EF Core contexts, and hosted services for event processing.
3. **UI Host** (`Application/UI`) — Blazor WASM frontend. Calls the API via HTTP. Each feature module contributes UI components and menu items.
4. **Feature modules** — Isolated vertical slices (Inventory, Materials, Models, Projects, Subscriptions). Each module owns its domain, API endpoints, UI components, and cross-module integration contracts.
5. **Persistence** — Two EF Core contexts:
   - `DatabaseContext` — Main business data. Instrumented with `OutboxSaveChangesInterceptor` to write domain events atomically on `SaveChanges()`.
   - `MessagingDbContext` — Inbox/outbox tables for event processing. A "Silent" keyed variant is used by polling hosted services to avoid excessive telemetry noise.

## Flow

- User interacts with **Blazor UI** → calls **API** over HTTPS
- **API endpoints** (in `*.Web` projects) call **domain services** (in `*.Core`)
- Domain operations emit **domain events** → written to **outbox table** atomically via interceptor
- **DomainEventDispatcher** (hosted service) polls outbox → dispatches events to **in-process handlers**
- Handlers may write **integration events** to the **integration outbox** (`IIntegrationOutbox`)
- **IntegrationEventPublisher** (hosted service) polls integration outbox → publishes to **RabbitMQ** (`ppm-events` exchange)
- **RabbitMqSubscriber** consumes events from RabbitMQ → routes to **integration event handlers** in `*.Integrations` projects

For **synchronous cross-module communication**, use `IDispatcher` commands/queries implemented in `*.Integrations` projects. See the "Cross-module communication" section below.

---

# Event-driven communication

The application uses the **inbox/outbox pattern** to ensure reliable, transactional event processing.

## Domain events (in-process)

1. **Domain operation runs** — e.g., user creates a `PaintBrand`
2. **SaveChanges() called** — `OutboxSaveChangesInterceptor` intercepts the call, writes domain event to the `outbox` table atomically with the business data
3. **DomainEventDispatcher** (hosted service) polls the outbox table every few seconds
4. **Domain event dispatched** in-process to registered handlers (same module or cross-module)
5. Handlers may write **integration events** to `IIntegrationOutbox` if other modules or external systems need to react

## Integration events (RabbitMQ)

1. **Integration event written** to the integration outbox table (via `IIntegrationOutbox`)
2. **IntegrationEventPublisher** (hosted service) polls the integration outbox table
3. **Published to RabbitMQ** via `ResilientBrokerPublisher` (wraps `RabbitMqPublisher` with resilience)
4. **RabbitMqSubscriber** receives events from the `ppm-events` exchange
5. **Routed to integration event handlers** in `*.Integrations` projects

## When to use events

Use **domain events** for:
- Same-module reactions to domain changes
- Cross-module async notifications (write to integration outbox from the domain event handler)

Use **integration events** for:
- Cross-module data projections (e.g., Materials → Projects material cache)
- External system notifications
- Anything that should survive module restarts independently

---

# Cross-module communication

Modules communicate in two ways:

## 1. Synchronous (IDispatcher)

For operations where the caller **must wait for the result** (e.g., validation, immediate query, transactional consistency):

- Caller uses `IDispatcher` to send a command/query
- Handler lives in the target module's `*.Integrations` project
- Request/result types live in `*.Integrations.Contracts` (shared)

Example: Projects module queries Materials for color match → `IDispatcher.Send<FindColorMatchesQuery>()`.

## 2. Asynchronous (Events)

For operations where the caller **does not need an immediate result** (e.g., cache updates, projections, notifications):

- Source module publishes an integration event (via `IIntegrationOutbox`)
- Target module subscribes to the event (handler in `*.Integrations`)

Example: Materials publishes `PaintColorCreated` → Projects updates its materials projection table.

**Guideline:** Prefer events for cross-module coupling. Use `IDispatcher` only when synchronous behavior is required.

---

# Shared infrastructure

## rbkApiModules

A third-party/shared library (referenced by `Application/Api`) that provides:
- **Core API setup** — CORS, HSTS, compression, middleware pipeline
- **Relational identity** — JWT-based authentication, EF Core user store, roles, claims
- **Multi-tenancy** — `IRequestContext` / `RequestContext` extracts tenant from JWT
- **UI definitions** — `AddRbkUIDefinitions()` registers menu/UI metadata

## DatabaseContext vs MessagingDbContext

- **DatabaseContext** — Main business data. Configured with `OutboxSaveChangesInterceptor` to write domain events atomically. Uses migrations in `Persistance/Database/Migrations/`.
- **MessagingDbContext** — Inbox/outbox tables for event processing. Separate context to avoid coupling business data migrations with messaging infrastructure. A "Silent" keyed variant is registered for polling services to reduce telemetry noise.

## Telemetry

OpenTelemetry is configured in `Application/Api/Program.cs` with:
- ASP.NET Core instrumentation (HTTP requests, routing)
- EF Core instrumentation (SQL queries)
- Npgsql instrumentation (PostgreSQL driver)

Traces are exported to the Aspire dashboard (`https://localhost:17257`).

## File storage

- `IFileStorage` / `LocalFileStorage` provides file storage per tenant
- `StorageQuota` feature tracks usage per tenant
- Files are stored on the local file system (path configured in `appsettings.json`)

## Multi-tenancy

- Every request is scoped to a **tenant** via `IRequestContext`
- JWT token contains tenant claim (`sub` or custom claim)
- EF Core queries are automatically scoped via global query filters (configured in `DatabaseContext`)

---

# Running tests

The solution uses **TUnit** with `Microsoft.Testing.Platform`.

From `src/`:
```bash
dotnet test --solution PaintingProjectsManagment.slnx
```

## Filtering tests

TUnit does **not** support the standard `--filter` flag. Use `--treenode-filter` instead:
```bash
dotnet test --treenode-filter "FullyQualifiedName~Inventory"
```

## Integration tests

- Use **Testcontainers** to spin up ephemeral PostgreSQL and RabbitMQ containers
- Automatically start/stop containers per test class
- No manual Docker setup required (Testcontainers handles everything)

## Playwright UI tests

Require browser binaries. After building, from `src/`:
```powershell
pwsh bin/Debug/net10.0/playwright.ps1 install
```
(Adjust path for your `$(Configuration)` and `$(TargetFramework)`.)

Tests will fail without installed browsers.

---

# Seeded accounts

The database is seeded on startup with development accounts:

| Username | Password | Tenant |
|----------|----------|--------|
| `rodrigo.basniak` | `trustno1` | RODRIGO.BASNIAK |
| `ricardo.smarzaro` | `zemiko987` | RICARDO.SMARZARO |
| `superuser` | `admin` | (admin, all tenants) |

Use these accounts to sign in at `https://localhost:7114/signin`.