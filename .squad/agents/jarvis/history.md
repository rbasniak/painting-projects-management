# Jarvis History

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

