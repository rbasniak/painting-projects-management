# Stark History

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

### 2025-01 — Full Architecture Review

Conducted comprehensive architecture review at Rodrigo's request.

**Overall verdict:** Solid architecture with clean foundations. The vertical slice + modular monolith pattern is well-implemented. The event-driven infrastructure is sophisticated. Some boundary violations exist but are fixable.

**Key strengths identified:**
1. Two-tier event system (domain → integration) with versioning is exemplary
2. Rich domain models with encapsulated behavior (Material.UpdateDetails, RaiseDomainEvent pattern)
3. UI properly isolated — references only *.Contracts projects
4. Integration tests use Testcontainers with event assertion helpers
5. Subscription tier policy catalog is a clean domain pattern

**Boundary violations found:**
1. `Inventory.Web` → `Inventory.UI` reference exists but is unused (dead reference)
2. `DatabaseContext` has direct FK from `MaterialForProject` → `Materials.Material` (leaks boundary at DB level)
3. Models module has inconsistent Integrations structure (`Api/` vs `Internal/`)

**rbkApiModules coupling:**
- ~40 direct package references across the solution
- Provides: IDispatcher, event infrastructure, auth, multi-tenancy, validators, telemetry
- Risk: OutboxSaveChangesInterceptor, MessagingDbContext not visible in repo — debugging requires library source

**Technical debt signals:**
- 16+ TODOs (enum validation, IAuthenticationContext migration, value object conversions)
- Projects.UI.csproj includes `.codex1`, `.codex2`, `.cursor`, `.AntiGravity` backup files
- Thin Core.Tests coverage (most testing in Web.Tests)

**Recommendations recorded in `.squad/decisions/inbox/stark-arch-review-findings.md`**


