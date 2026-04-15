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

### Models Module: MustHave Bug Fix and SizeInMb Nullable (2026-04-15)

**Context:** Fixed missing `MustHave` property in create/update requests and changed `SizeInMb` from `int` to `int?` (nullable).

**What was found:**

1. **MustHave bug:**
   - The `Model` entity had a `MustHave` property and `SetMustHave()` method
   - `ModelDetails` and `IModelDetails` already exposed `MustHave`
   - BUT: `CreateModel.Request` and `UpdateModel.Request` did NOT have the `MustHave` property
   - Result: The property could never be set during creation or update
   - The handlers never called `SetMustHave()` to persist the value

2. **SizeInMb should be nullable:**
   - Originally `int` (non-nullable) with default value 0
   - Many models don't have a known file size, so nullable is more appropriate
   - Changed throughout the stack: entity, EF config, DTOs, requests, integration events

**Files modified:**

Models Module backend files:
- `Models.Core/Models/Domain/Model.cs` — changed `SizeInMb` property from `int` to `int?`, updated constructor and `UpdateDetails()` signatures
- `Models.Core/Database/ModelConfig.cs` — added `.IsRequired(false)` for `SizeInMb`
- `Models.Web/UseCases/Models/Commands/CreateModel.cs` — added `MustHave` property to Request, updated validator for nullable `SizeInMb`, added `SetMustHave()` call in handler
- `Models.Web/UseCases/Models/Commands/UpdateModel.cs` — added `MustHave` property to Request, updated validator for nullable `SizeInMb`, added `SetMustHave()` call in handler
- `Models.Web/DataTransfer/ModelDetails.cs` — changed `Size` property from `int` to `int?`
- `Models.Web.Contracts/DataTransfer/IModelDetails.cs` — changed `Size` property from `int` to `int?`
- `Models.Integrations.Contracts/Events/ModelEvents.cs` — updated `ModelCreatedV1` and `ModelUpdatedV1` records to use `int? SizeInMb`

EF Migration:
- `Persistance/Database/Migrations/20260415133032_AddModelSizeInMbNullableAndMustHave.cs` — alters `SizeInMb` column to nullable

**Key patterns observed:**

1. **Vertical slice completeness:** When adding/fixing a property, must check ALL layers:
   - Domain entity (`*.Core/Models/Domain/`)
   - EF configuration (`*.Core/Database/`)
   - Request classes (`*.Web/UseCases/*/Commands/` and `*.Web/UseCases/*/Queries/`)
   - Request validators (nested in use case classes)
   - Handlers (nested in use case classes) — must map request to entity AND call domain methods
   - Response DTOs (`*.Web/DataTransfer/`)
   - DTO interfaces (`*.Web.Contracts/DataTransfer/`)
   - Integration events (`*.Integrations.Contracts/Events/`)

2. **Domain methods vs direct assignment:**
   - The `Model` entity has a `SetMustHave()` method for the `MustHave` property
   - The `UpdateDetails()` method does NOT accept `mustHave` parameter (domain design choice — separate concern)
   - Handlers must call both `UpdateDetails()` AND `SetMustHave()` to fully update the entity
   - Don't assume `UpdateDetails()` updates everything — check the signature

3. **Nullable property validation:**
   - Use `.When(x => x.Property.HasValue)` in FluentValidation rules to only validate when value is present
   - Use null-coalescing operator `?? 0` when passing nullable to domain methods that expect non-nullable

4. **EF migration workflow:**
   - Migration must be generated from a project that references `Microsoft.EntityFrameworkCore.Design`
   - For this solution: run from `Application/Api` project with `--project` pointing to `Persistance/Database`
   - Command: `dotnet ef migrations add <Name> --project ..\..\Persistance\Database\PaintingProjectsManagment.Database.csproj`

**Build verification:**
- Initial build: 6 errors related to `int` vs `int?` mismatch in integration events
- After fixing integration events: Build succeeded with 33 warnings
- After migration generation: Build succeeded with 62 warnings (standard for this solution)

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

### Models Module: MustHave Bug Fix and SizeInMb Nullable (2026-04-15 — Current Session)

**Context:** Fixed missing `MustHave` property in create/update requests and changed `SizeInMb` from `int` to `int?` (nullable).

**What was found:**

1. **MustHave bug:**
   - The `Model` entity had a `MustHave` property and `SetMustHave()` method
   - `ModelDetails` and `IModelDetails` already exposed `MustHave`
   - BUT: `CreateModel.Request` and `UpdateModel.Request` did NOT have the `MustHave` property
   - Result: The property could never be set during creation or update
   - The handlers never called `SetMustHave()` to persist the value

2. **SizeInMb should be nullable:**
   - Originally `int` (non-nullable) with default value 0
   - Many models don't have a known file size, so nullable is more appropriate
   - Changed throughout the stack: entity, EF config, DTOs, requests, integration events

**Files modified:**

Models Module backend files:
- `Models.Core/Models/Domain/Model.cs` — changed `SizeInMb` property from `int` to `int?`, updated constructor and `UpdateDetails()` signatures
- `Models.Core/Database/ModelConfig.cs` — added `.IsRequired(false)` for `SizeInMb`
- `Models.Web/UseCases/Models/Commands/CreateModel.cs` — added `MustHave` property to Request, updated validator for nullable `SizeInMb`, added `SetMustHave()` call in handler
- `Models.Web/UseCases/Models/Commands/UpdateModel.cs` — added `MustHave` property to Request, updated validator for nullable `SizeInMb`, added `SetMustHave()` call in handler
- `Models.Web/DataTransfer/ModelDetails.cs` — changed `Size` property from `int` to `int?`
- `Models.Web.Contracts/DataTransfer/IModelDetails.cs` — changed `Size` property from `int` to `int?`
- `Models.Integrations.Contracts/Events/ModelEvents.cs` — updated `ModelCreatedV1` and `ModelUpdatedV1` records to use `int? SizeInMb`

EF Migration:
- `Persistance/Database/Migrations/20260415133032_AddModelSizeInMbNullableAndMustHave.cs` — alters `SizeInMb` column to nullable

**Key patterns observed:**

1. **Vertical slice completeness:** When adding/fixing a property, must check ALL layers:
   - Domain entity (`*.Core/Models/Domain/`)
   - EF configuration (`*.Core/Database/`)
   - Request classes (`*.Web/UseCases/*/Commands/` and `*.Web/UseCases/*/Queries/`)
   - Request validators (nested in use case classes)
   - Handlers (nested in use case classes) — must map request to entity AND call domain methods
   - Response DTOs (`*.Web/DataTransfer/`)
   - DTO interfaces (`*.Web.Contracts/DataTransfer/`)
   - Integration events (`*.Integrations.Contracts/Events/`)

2. **Domain methods vs direct assignment:**
   - The `Model` entity has a `SetMustHave()` method for the `MustHave` property
   - The `UpdateDetails()` method does NOT accept `mustHave` parameter (domain design choice — separate concern)
   - Handlers must call both `UpdateDetails()` AND `SetMustHave()` to fully update the entity
   - Don't assume `UpdateDetails()` updates everything — check the signature

3. **Nullable property validation:**
   - Use `.When(x => x.Property.HasValue)` in FluentValidation rules to only validate when value is present
   - Use null-coalescing operator `?? 0` when passing nullable to domain methods that expect non-nullable

4. **EF migration workflow:**
   - Migration must be generated from a project that references `Microsoft.EntityFrameworkCore.Design`
   - For this solution: run from `Application/Api` project with `--project` pointing to `Persistance/Database`
   - Command: `dotnet ef migrations add <Name> --project ..\..\Persistance\Database\PaintingProjectsManagment.Database.csproj`

**Build verification:**
- Initial build: 6 errors related to `int` vs `int?` mismatch in integration events
- After fixing integration events: Build succeeded with 33 warnings
- After migration generation: Build succeeded with 62 warnings (standard for this solution)

**Coordination with teammates:**
- **Loki (Frontend):** Needs updated request models with `MustHave` and nullable `SizeInMb`; consumes backend changes
- **Hulk (Tester):** Created 10 proactive integration tests validating persistence of both fields

**Session artifacts:**
- Orchestration: `.squad/orchestration-log/2026-04-15T13-38-06Z-jarvis.md`
- Session log: `.squad/log/2026-04-15T13-38-06Z-model-fields.md`

