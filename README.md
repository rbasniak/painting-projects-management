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
- `Models/` – domain entities (e.g. `PaintBrand`, `PaintLine`, `PaintColor`, ...)
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