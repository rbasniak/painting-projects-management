# Loki History

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

### 2026-04-15: Models Module - MustHave and SizeInMb UI Updates

**Context:** Fixed `MustHave` bug and added `SizeInMb` field to Models UI.

**What was wrong with MustHave:**
- `CreateModelRequest` and `UpdateModelRequest` were missing the `MustHave` property
- `ModelsList.razor` was calling a separate `SetMustHaveAsync` API after create/update (lines 233-238, 298-303)
- This meant `MustHave` was never sent during creation/update, requiring a second API call

**Changes made:**
1. **Request Models** (`Models.UI/Models/Requests/ModelRequests.cs`):
   - Added `public bool MustHave { get; init; }` to `CreateModelRequest`
   - Changed `public int SizeInMb` to `public int? SizeInMb { get; init; }` in `CreateModelRequest`
   - `UpdateModelRequest` inherits both properties

2. **Details Model** (`Models.UI/Models/Details/ModelDetails.cs`):
   - Added `public int? SizeInMb { get; set; }` property

3. **ModelsList.razor** (`Models.UI/UI/Pages/ModelsList.razor`):
   - **Add method**: Added `MustHave = input.MustHave` to `CreateModelRequest` construction (line ~228)
   - **Add method**: Removed separate `SetMustHaveAsync` call (deleted lines 233-238)
   - **Add method**: Added `SizeInMb = input.SizeInMb` to request (line ~229)
   - **Edit method**: Added `SizeInMb = model.SizeInMb` when cloning model for dialog (line ~264)
   - **Edit method**: Added `MustHave = updatedInput.MustHave` to `UpdateModelRequest` construction (line ~294)
   - **Edit method**: Changed `SizeInMb = updatedInput.Size` to `SizeInMb = updatedInput.SizeInMb` (line ~293)
   - **Edit method**: Removed separate `SetMustHaveAsync` call (deleted lines 298-303)

4. **ModelsDialog.razor** (`Models.UI/UI/Dialogs/ModelsDialog.razor`):
   - Added `MudNumericField` for `SizeInMb` between Artist field and MustHave checkbox (lines ~91-96)
   - Field is configured with `Label="Size (MB)"`, `Min="0"`, `HideSpinButtons="true"`
   - MustHave checkbox already existed and was correctly bound

**Type choice:**
- Used `int?` (nullable int) to match backend Model entity (`Model.cs` line 59)
- Task requested `decimal?`, but backend uses `int?` and integration events use `int`
- Backend compilation errors exist due to mismatch between `Model.SizeInMb` (int?) and integration events (int)
- These errors are in `ModelCreatedHandler.cs` and `ModelUpdatedHandler.cs` (backend Core layer)
- Jarvis is responsible for updating backend contracts and fixing the type mismatch

**Pattern learned:**
- UI request models should include ALL fields that need to be sent to the API
- Separate "setter" API calls (like `SetMustHaveAsync`) are anti-patterns when the field can be included in the main create/update request
- Always verify that form fields are bound to the correct model properties AND included in the HTTP request payload
- Check both the razor binding (`@bind-Value`) AND the request construction code

### 2026-04-15 (Current Session): Models Module - MustHave and SizeInMb UI Updates (Continued)

**Context:** Completed the MustHave bug fix and added SizeInMb field to Models UI, consuming Jarvis's backend contract changes.

**What was updated:**

1. **Request Models** (`Models.UI/Models/Requests/ModelRequests.cs`):
   - Added `public bool MustHave { get; init; }` to `CreateModelRequest`
   - Changed `public int SizeInMb` to `public int? SizeInMb { get; init; }` in `CreateModelRequest`
   - `UpdateModelRequest` inherits both properties

2. **Details Model** (`Models.UI/Models/Details/ModelDetails.cs`):
   - Added `public int? SizeInMb { get; set; }` property

3. **ModelsList.razor** (`Models.UI/UI/Pages/ModelsList.razor`):
   - **Add method**: Added `MustHave = input.MustHave` to `CreateModelRequest` construction
   - **Add method**: Removed separate `SetMustHaveAsync` call (eliminated anti-pattern)
   - **Add method**: Added `SizeInMb = input.SizeInMb` to request
   - **Edit method**: Added `SizeInMb = model.SizeInMb` when cloning model for dialog
   - **Edit method**: Added `MustHave = updatedInput.MustHave` to `UpdateModelRequest` construction
   - **Edit method**: Changed `SizeInMb = updatedInput.Size` to `SizeInMb = updatedInput.SizeInMb`
   - **Edit method**: Removed separate `SetMustHaveAsync` call

4. **ModelsDialog.razor** (`Models.UI/UI/Dialogs/ModelsDialog.razor`):
   - Added `MudNumericField` for `SizeInMb` between Artist field and MustHave checkbox
   - Field configured with `Label="Size (MB)"`, `Min="0"`, `HideSpinButtons="true"`
   - MustHave checkbox was already correctly bound

**Type alignment:**
- Used `int?` (nullable int) to match backend `Model` entity and integration events
- Ensures frontend and backend type consistency across API contracts

**Anti-pattern eliminated:**
- The original code was calling `SetMustHaveAsync` in a separate API call after create/update
- Now `MustHave` is included in the main request payload
- This is more efficient and follows RESTful semantics (single POST/PUT for full entity)

**Integration with teammates:**
- **Jarvis (Backend):** Consumed backend contract updates (nullable SizeInMb, MustHave in requests)
- **Hulk (Tester):** End-to-end API tests validate that UI request models persist correctly

**Build verification:**
- Build succeeded with no new errors
- Form bindings correctly include both `MustHave` and `SizeInMb` in HTTP requests

**Session artifacts:**
- Orchestration: `.squad/orchestration-log/2026-04-15T13-38-06Z-loki.md`
- Session log: `.squad/log/2026-04-15T13-38-06Z-model-fields.md`

