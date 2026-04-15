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

### Layer Violation Fixes (2026-04-15)

**Context:** Fixed three module boundary violations identified by Stark during architecture review.

**What was found:**

1. **Projects.Web → Subscriptions.Core violation**
   - `Projects.Web.csproj` had a direct reference to `Subscriptions.Core`
   - Three use cases were using `GetSubscriptionEntitlementQuery` (correct - from Integrations.Contracts)
   - One use case (`UploadProjectReferencePicture`) was directly injecting `ISubscriptionTierPolicyCatalog` from `Subscriptions.Core` and using it as a fallback when the dispatcher query failed
   - This violated the rule: `*.Web` should never reference another module's `*.Core` directly

2. **Inventory.Web → Inventory.UI violation**
   - `Inventory.Web.csproj` had a reference to `Inventory.UI`
   - No actual code was using this reference (dead reference)
   - Violates layer direction: Web should never reference UI

3. **Projects.UI artifact files**
   - `.csproj` had explicit `<Content Include>` entries for AI/code review artifacts: `.codex1`, `.codex2`, `.cursor`, `.AntiGravity`
   - These files existed in `UI\Dialogs\` directory

**Contracts that already existed:**
- `GetSubscriptionEntitlementQuery` was already properly defined in `Subscriptions.Integrations.Contracts`
- `SubscriptionEntitlementResult` with all necessary entitlement data (MaxActiveProjects, MaxProjectReferencePicturesPerProject, etc.)
- `SubscriptionTier` enum was already in `Subscriptions.Core.Contracts` (shared types)

**Key insight:**
- The violation wasn't about missing contracts — the dispatcher pattern was already in place
- The issue was using a Core service (`ISubscriptionTierPolicyCatalog`) as a fallback instead of trusting the Integration layer
- The fix: "fail open" when entitlement query fails (return `true` to allow operation) rather than reaching into another module's Core layer for defaults

**Cross-module communication patterns confirmed:**
- ✅ Use `IDispatcher` with queries from `*.Integrations.Contracts` for sync cross-module calls
- ✅ Reference `*.Core.Contracts` for shared type definitions (enums, constants)
- ❌ Never inject services or use implementations from another module's `*.Core`
- ❌ Never reference `*.UI` from `*.Web` (inverts layer direction)

**Files modified during session:**
- `Projects.Web.csproj` — removed Subscriptions.Core reference, added Core.Contracts
- `UploadProjectReferencePicture.cs` — changed to fail-open pattern
- `Inventory.Web.csproj` — removed Inventory.UI dead reference
- `Projects.UI.csproj` — removed artifact file entries, deleted 4 files from disk

**Session artifacts:**
- Orchestration: `.squad/orchestration-log/2026-04-15T11-43-58Z-jarvis.md`
- Session log: `.squad/log/2026-04-15T11-43-58Z-violation-fixes-and-arch-tests.md`
- Decision: Merged to `.squad/decisions.md`

