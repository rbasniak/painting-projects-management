# Wong History

## Project Context (Day 1)

**Project:** Painting Projects Management (ppm)
**Owner:** Rodrigo Basniak
**Stack:** .NET 10, ASP.NET Core, Blazor WASM, PostgreSQL, EF Core, RabbitMQ, .NET Aspire
**Architecture:** Vertical slices with module isolation. Event-driven cross-module communication via inbox/outbox pattern (RabbitMQ). .NET Aspire orchestrates all services.

**Module layout (per feature):**
- `*.Core` — domain entities, domain services, EF Core config, domain events
- `*.Core.Contracts` — cross-module shared types (enums, constants)
- `*.Core.Tests` — pure unit tests, no external dependencies
- `*.Web` — ASP.NET Core endpoints, vertical slices in `UseCases/`, DTOs in `DataTransfer/`
- `*.Web.Contracts` — `I*Details` interfaces and request interfaces shared between API and Blazor
- `*.UI` — Blazor WASM: services, models, menu/route registration
- `*.Integrations` — cross-module `IDispatcher` commands/queries (sync cross-module comms)
- `*.Integrations.Contracts` — shared request/result types for cross-module ops

**Reference rules:** UI → Web.Contracts + Core.Contracts only. No direct Core references from UI.
**Testing:** TUnit + Testcontainers. Use `--treenode-filter` (not `--filter`). Run from `src/`.
**Solution:** `src/PaintingProjectsManagment.slnx`

## Learnings

### Inbox/Outbox Pattern (Full Stack)
- **Two-tier outbox:** Domain events (in-process, same database) → Integration events (RabbitMQ, cross-module/external)
- **Atomic writes:** `OutboxSaveChangesInterceptor` writes domain events to outbox table during `SaveChanges()` — no separate transaction
- **Two hosted services:** `DomainEventDispatcher` polls domain outbox → in-process handlers. `IntegrationEventPublisher` polls integration outbox → RabbitMQ
- **Silent context pattern:** `MessagingDbContext` has a "Silent" keyed variant for polling services to avoid excessive telemetry noise

### Aspire Orchestration
- AppHost wires: PostgreSQL (with pgAdmin), RabbitMQ (with management UI), API (waits for both), UI (waits for API)
- Service discovery built-in — no hardcoded URLs between services
- `--allow-unsecured-transport` needed for dev/headless environments (HTTPS cert issues)
- **HTTPS critical for UI:** API enforces HTTPS redirect, so Blazor WASM must be served via HTTPS or API calls fail silently

### Two EF Contexts Pattern
- `DatabaseContext` — business data + outbox interceptor + migrations
- `MessagingDbContext` — inbox/outbox tables only, separate to avoid coupling business migrations with messaging infrastructure
- Both contexts share the same database but have independent migration histories

### Cross-Module Communication Strategy
- **Sync:** `IDispatcher` commands/queries in `*.Integrations` (request/result in `*.Integrations.Contracts`)
- **Async:** Integration events via RabbitMQ (handlers in `*.Integrations`)
- **Guideline:** Prefer events (loose coupling). Use dispatcher only when caller must wait for result.

### rbkApiModules Shared Library
- Provides: JWT auth, relational identity (EF Core user store), multi-tenancy (`IRequestContext`), UI definitions
- Reduces boilerplate in `Application/Api/Program.cs`
- Keeps third-party/shared infrastructure out of feature modules

### TUnit Testing Framework
- Uses `Microsoft.Testing.Platform` instead of xUnit/NUnit runners
- **Does NOT support `--filter`** — use `--treenode-filter` instead
- Testcontainers pattern for integration tests (ephemeral PostgreSQL + RabbitMQ)
- Playwright tests require `pwsh bin/.../playwright.ps1 install` for browser binaries

