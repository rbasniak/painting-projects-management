# Hulk History

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

### Architecture Tests with ArchUnitNET (2026-04-15 — corrected from NetArchTest.Rules)

Created `src/Features/Architecture.Tests/` project to automatically enforce module boundary rules in CI.

**Package choice:**
- Initial implementation used `NetArchTest.Rules` v1.3.2 — WRONG, rejected by user
- **Correct library: `TngTech.ArchUnitNET` v0.13.3** per user directive
- Added to `Directory.Packages.props` for central package management

**ArchUnitNET pattern for TUnit (no xUnit/NUnit adapter):**
- Assemblies loaded once: `new ArchLoader().LoadAssemblies(...).Build()` → stored as `ArchitectureModel` (alias to avoid `Architecture` namespace clash)
- Rules declared as `IArchRule`: `Classes().That().ResideInNamespace("A").Should().NotDependOnAnyTypesThat().ResideInNamespace("B")`
- `Check()` lives in xUnit/NUnit adapters — NOT available standalone. Use `rule.HasNoViolations(Arch)` instead
- `ArchitectureModel = ArchUnitNET.Domain.Architecture` alias required — `Architecture` clashes with namespace name

**Test structure:**
- Tests reference actual types from modules to load assemblies: `typeof(Material).Assembly`
- Use `ResideInNamespace` (ArchUnitNET does prefix matching natively)
- Fully qualified type names required when there are naming conflicts

**Key gotchas:**
- Project file structure: each layer has a subfolder (e.g., `Materials/Core/Core/`, `Materials/Web/Web/`)
- Namespaces vary: Materials DTOs use `*.UseCases.Web`, Projects DTOs use just `*.Features.Projects`
- Must reference enough projects in .csproj to load all assemblies under test
- TUnit assertions work great with `.ShouldBeTrue()` from Shouldly

**Rules enforced:**
1. UI projects don't reference Core directly (use Core.Contracts)
2. UI projects don't reference Web use cases (use Web.Contracts)
3. Web projects don't reference other modules' Core (cross-module violation — Jarvis fixing)
4. Core projects don't reference Web or UI
5. All modules follow naming convention (PaintingProjectsManagement.Features.* or .UI.Modules.*)
6. UI only references Contracts projects

**Run tests:**
```
cd src/
dotnet test --project Features\Architecture.Tests\Architecture.Tests.csproj
```

All tests passed ✅ except the known violation (Projects.Web → Subscriptions.Core) that Jarvis is fixing.

## Session Work (2026-04-15)

**Spawned as:** Part of two-agent violation fix + tests session

**Artifacts created:**
- `src/Features/Architecture.Tests/Architecture.Tests.csproj` — TUnit test project
- `src/Features/Architecture.Tests/ModuleBoundaryTests.cs` — 6 ArchUnitNET tests (corrected from NetArchTest.Rules)
- Modified: `src/Directory.Packages.props` — added TngTech.ArchUnitNET v0.13.3 (replaced NetArchTest.Rules)

**Test results:** All 7 rules pass ✅

**Tests enforce:**
1. UI → Core.Contracts only (no direct Core)
2. UI → Web.Contracts only (no Web.UseCases)
3. Web ↛ other modules' Core (Projects.Web ↛ Subscriptions.Core — pre-existing, Jarvis fixed)
4. Core ↛ Web/UI (no inverted deps)
5. Naming convention (PaintingProjectsManagement.Features.*)
6. UI only refs Contracts (comprehensive)
7. Integrations don't ref other modules' Core

**Session artifacts:**
- Orchestration: `.squad/orchestration-log/2026-04-15T11-43-58Z-hulk.md`
- Session log: `.squad/log/2026-04-15T11-43-58Z-violation-fixes-and-arch-tests.md`
- Decision: Merged to `.squad/decisions.md`

