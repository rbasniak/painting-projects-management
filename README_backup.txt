![Coverage Badge](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/rbasniak/a1778faea690a8a406f92dd302cca1cf/raw/coverage-badge.json)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/actions)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

[Code coverage report](https://rbasniak.github.io/painting-projects-management/)

# Painting Projects Management

A comprehensive .NET solution for managing painting projects, materials, models, and paints. This solution follows a modular architecture with clear separation of concerns and robust testing patterns.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Solution Structure](#solution-structure)
- [Module Architecture](#module-architecture)
- [Feature Modules](#feature-modules)
- [Module Communication](#module-communication)
- [Testing Strategy](#testing-strategy)
- [Development Guidelines](#development-guidelines)
- [Getting Started](#getting-started)

## Architecture Overview

The solution follows a **Feature-Based Modular Architecture** with the following key principles:

- **Separation of Concerns**: Each feature is self-contained with its own models, use cases, and tests
- **Dependency Inversion**: Features communicate through abstractions, not concrete implementations
- **Multi-Tenant Support**: Built-in tenant isolation for data security
- **Clean Architecture**: Clear boundaries between domain, application, and infrastructure layers

### Core Architecture Layers

```
┌─────────────────────────────────────┐
│           API Layer                 │
│   (PaintingProjectsManagement.Api)  │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│         Feature Modules             │
│  (Materials, Paints, Models, etc.) │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│        Abstractions Layer           │
│  (Module.Abstractions projects)     │
└─────────────────────────────────────┘
┌─────────────────────────────────────┐
│        Infrastructure Layer         │
│      (Database, ServiceDefaults)    │
└─────────────────────────────────────┘
```

## Solution Structure

### Main Projects

- **`PaintingProjectsManagement.Api`**: Main API entry point
- **`PaintingProjectsManagement.AppHost`**: Application host for Aspire
- **`PaintingProjectsManagement.ServiceDefaults`**: Shared service configurations
- **`PaintingProjectsManagment.Database`**: Database context and migrations

### Feature Modules

Each feature follows a consistent structure:

```
PaintingProjectsManagement.Features.{FeatureName}/
├── Database/
│   └── {Entity}Config.cs          # EF Core configurations
├── DataTransfer/
│   ├── {Entity}Details.cs         # Response DTOs
│   └── Mappers/
│       └── {Entity}To{Dto}.cs     # Mapping extensions
├── Models/
│   └── {Entity}.cs                # Domain entities
├── UseCases/
│   ├── Builder.cs                 # Feature registration
│   ├── Commands/
│   │   ├── Create{Entity}.cs
│   │   ├── Update{Entity}.cs
│   │   └── Delete{Entity}.cs
│   └── Queries/
│       ├── List{Entity}s.cs
│       └── Get{Entity}Details.cs
└── Integrations/
    └── {Integration}.cs           # Cross-module integrations
```

### Abstractions Projects

- **`PaintingProjectsManagement.Features.{FeatureName}.Abstractions`**: Public contracts and DTOs for inter-module communication

### Test Projects

- **`PaintingProjectsManagement.Features.{FeatureName}.Tests`**: Comprehensive test suites for each feature

## Module Architecture

### Feature Module Structure

Each feature module is self-contained and follows these patterns:

#### 1. Domain Models
```csharp
public class Material : TenantEntity
{
    // EF Core requires a private empty constructor
    private Material() { }

    // Primary constructor must create a valid state
    public Material(string tenantId, string name, MaterialUnit unit, double pricePerUnit)
    {
        TenantId = tenantId;
        Name = name;
        Unit = unit;
        PricePerUnit = pricePerUnit;
    }

    // All properties must have private setters
    public string Name { get; private set; } = string.Empty;
    public MaterialUnit Unit { get; private set; }
    public double PricePerUnit { get; private set; }

    // Methods should be named after use cases, not setters
    public void UpdateDetails(string name, MaterialUnit unit, double pricePerUnit)
    {
        Name = name;
        Unit = unit;
        PricePerUnit = pricePerUnit;
    }

    // Example of use case naming: Activate/Deactivate instead of SetActive/SetInactive
    public void Activate() { /* activation logic */ }
    public void Deactivate() { /* deactivation logic */ }
}

// Example with collections
public class Project : TenantEntity
{
    private readonly HashSet<Material> _materials = new();

    private Project() { } // EF Core constructor

    public Project(string tenantId, string name) // Primary constructor
    {
        TenantId = tenantId;
        Name = name;
        // Collections are initialized only in primary constructor
    }

    public string Name { get; private set; } = string.Empty;
    
    // Collection properties backed by private HashSet
    public IReadOnlyCollection<Material> Materials => _materials.AsReadOnly();

    public void AddMaterial(Material material)
    {
        _materials.Add(material);
    }

    public void RemoveMaterial(Material material)
    {
        _materials.Remove(material);
    }
}
```

#### 2. Database Configuration
```csharp
public class MaterialConfig : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(m => new { m.Name, m.TenantId }).IsUnique();
    }
}
```

#### 3. Use Cases
```csharp
public class CreateMaterial : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/materials", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<MaterialDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Create Material")
        .WithTags("Materials");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string Name { get; set; } = string.Empty;
        public MaterialUnit Unit { get; set; }
        public double PricePerUnit { get; set; }
    }

    public class Validator : SmartValidator<Request, Material>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // SmartValidator automatically validates DB constraints, PKs, and FKs
            // Focus only on business logic validation
            RuleFor(x => x.Name)
                .MustAsync(async (request, name, cancellationToken) =>
                {
                    return !await Context.Set<Material>().AnyAsync(m => 
                        m.Name == name && m.TenantId == request.Identity.Tenant, cancellationToken);
                })
                .WithMessage("A material with this name already exists.");

            RuleFor(x => x.PricePerUnit)
                .GreaterThan(0)
                .WithMessage("Price per unit must be greater than zero.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        // Handler receives validated request - no additional checks needed
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var material = new Material(
                request.Identity.Tenant,
                request.Name, 
                request.Unit, 
                request.PricePerUnit
            );

            await _context.AddAsync(material, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var result = MaterialDetails.FromModel(material);
            return CommandResponse.Success(result);
        }
    }
}

// Example of non-entity validator
public class GetMaterialStatistics : IEndpoint
{
    public class Request : AuthenticatedRequest, IQuery
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Validator : AbstractValidator<Request> // Not SmartValidator for non-entity requests
    {
        public Validator()
        {
            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Start date must be before end date.");
        }
    }
}
```

#### 4. Feature Registration
```csharp
public static class Builder
{
    public static IEndpointRouteBuilder MapMaterialsFeature(this IEndpointRouteBuilder app)
    {
        CreateMaterial.MapEndpoint(app);
        UpdateMaterial.MapEndpoint(app);
        DeleteMaterial.MapEndpoint(app);
        ListMaterials.MapEndpoint(app);
        return app;
    }
}
```

## Feature Modules

### Materials Module
- **Purpose**: Manage painting materials (brushes, canvases, etc.)
- **Key Features**: CRUD operations, unit management, pricing
- **Abstractions**: `ReadOnlyMaterial`, `GetMaterialsForProject`

### Paints Module
- **Purpose**: Manage paint brands, colors, and lines
- **Key Features**: Color matching, brand management, paint line organization
- **Services**: `ColorMatchingService`

### Models Module
- **Purpose**: Manage 3D models and figures
- **Key Features**: Model categories, size management, figure types
- **Sub-modules**: Models, ModelCategories

### Projects Module
- **Purpose**: Manage painting projects and workflows
- **Key Features**: Project management, color sections, material allocation
- **Complex Features**: Color groups, zones, and sections

## Module Communication

### Inter-Module Communication Pattern

Modules communicate through **Abstractions** projects, following the Dependency Inversion Principle:

#### 1. Abstractions Contract
```csharp
// PaintingProjectsManagement.Features.Materials.Abstractions
public class ReadOnlyMaterial
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public MaterialUnit Unit { get; init; }
    public double PricePerUnit { get; init; }
}

public static partial class GetMaterialsForProject 
{
    public sealed class Request : AuthenticatedRequest, IQuery<IReadOnlyCollection<ReadOnlyMaterial>>
    {
        public Guid[] MaterialIds { get; set; } = Array.Empty<Guid>();
    }
}
```

#### 2. Implementation
```csharp
// PaintingProjectsManagement.Features.Materials.Integrations
public static partial class GetMaterialsForProject
{
    public sealed class Handler(DbContext _context) : ITypedQueryHandler<Abstractions.GetMaterialsForProject.Request, IReadOnlyCollection<ReadOnlyMaterial>>
    {
        public async Task<QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>> HandleAsync(Abstractions.GetMaterialsForProject.Request request, CancellationToken cancellationToken)
        {
            var materials = await _context.Set<Material>()
                .Where(x => x.TenantId == request.Identity.Tenant)
                .Where(x => request.MaterialIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var results = materials.Select(x => x.MapFromModel()).AsReadOnly();
            return QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>.Success(results);
        }
    }
}
```

#### 3. Mapping
```csharp
// PaintingProjectsManagement.Features.Materials.DataTransfer.Mappers
public static class MaterialToReadOnlyMaterial
{
    public static ReadOnlyMaterial MapFromModel(this Material model)
    {
        return new ReadOnlyMaterial
        {
            Id = model.Id,
            Name = model.Name,
            Unit = model.Unit,
            PricePerUnit = model.PricePerUnit
        };
    }
}

// Note: Mapping is done in the module project, not in abstractions
// because abstractions don't have access to domain models
```

### Communication Rules

1. **No Direct Dependencies**: Modules should not directly reference other modules
2. **Abstractions Only**: Use only the `.Abstractions` projects for inter-module communication
3. **Request/Response Pattern**: All cross-module communication uses request/response pattern
4. **Tenant Isolation**: All requests must include tenant context for security

## Testing Strategy

### Test Structure

Each feature has a dedicated test project following this structure:

```
PaintingProjectsManagement.Features.{FeatureName}.Tests/
├── TestingServer.cs              # Custom test server setup
├── GlobalSetup.cs                # Global test configuration
├── UseCases/
│   ├── Create_{Entity}_Tests.cs
│   ├── Update_{Entity}_Tests.cs
│   ├── Delete_{Entity}_Tests.cs
│   └── List_{Entity}s_Tests.cs
└── Integrations/
    └── {Integration}_Tests.cs
```

### Testing Patterns

#### 1. Test Server Setup
```csharp
public class TestingServer : RbkTestingServer<Program>
{
    protected override bool UseHttps => true;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
    }
}
```

#### 2. Comprehensive Test Coverage
```csharp
public class Create_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // 1. Store credentials for use across tests
        await TestingServer.CacheCredentialsAsync("user1", "password", "tenant1");
        await TestingServer.CacheCredentialsAsync("user2", "password", "tenant2");;

        // 2. Initialize test entities
        var existingMaterial = new Material("tenant1", "Existing Material", MaterialUnit.Unit, 10.0);
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingMaterial);
            await context.SaveChangesAsync();
        }
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Material()
    {
        // Authentication test - always 2nd
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Create_Material_When_Name_Is_Empty()
    {
        // Validation tests - always 3rd+
    }

    [Test, NotInParallel(Order = 14)]
    public async Task User_Can_Create_Material()
    {
        // Success tests - always last before cleanup
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        // Always last - delete database
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}
```

### Testing Guidelines

1. **Test Structure**: Each use case has its own test file
2. **Test Ordering**: 
   - Order 1: `Seed()` - Initialize credentials and test data
   - Order 2-3: Authentication and authorization tests
   - Order 4+: Validation tests (all validation rules and edge cases)
   - Order 14+: Success tests (happy path scenarios)
   - Order 99: `CleanUp()` - Delete database
3. **Database Isolation**: Each test class uses a separate database instance
4. **Authentication Testing**: Test both authenticated and unauthenticated scenarios
5. **Validation Testing**: Test all validation rules and edge cases
6. **Business Logic Testing**: Verify business rules and constraints
7. **Database Verification**: Always verify database state after operations

### Test Categories

- **Authentication Tests**: Verify proper authentication and authorization
- **Validation Tests**: Test input validation and business rules
- **Success Tests**: Verify correct behavior for valid inputs
- **Integration Tests**: Test cross-module communication
- **Edge Case Tests**: Test boundary conditions and error scenarios

## Development Guidelines

### Architectural Rules

#### Domain Models
- **Private Setters**: All properties must have private setters
- **EF Core Constructor**: Must have a private empty constructor for EF Core
- **Primary Constructor**: Must create a valid state when constructed
- **Method Naming**: Avoid `SetX()` methods, use use case names like `.Activate()`, `.Deactivate()`
- **Collections**: Must be backed by private `HashSet<T>` and initialized only in primary constructor

#### Use Cases
- **Class Structure**: Use case name as class with nested `Request`, `Validator`, `Handler`, and optional `Response`
- **Endpoint Pattern**: Simple call to `IDispatcher` with request
- **Return Pattern**: Always `return ResultsMapper.FromResponse(result);`
- **Endpoint Requirements**: Minimum `AllowAnonymous`/`RequireAuthorization`, `WithName`, `WithTags`, `Produces`
- **Validator Types**: 
  - Use `SmartValidator` for database entity requests (auto-validates DB constraints)
  - Use `AbstractValidator` for non-entity requests
- **Validation Strategy**: All validation in validator, handlers receive validated requests
- **Use Case Organization**:
  - Frontend use cases: `UseCases/` folder
  - Integration use cases: `Integrations/` folder (queries only)

#### DTOs
- **Frontend DTOs**: `{Entity}Header` or `{Entity}Details` (e.g., `MaterialDetails`)
- **Integration DTOs**: `ReadOnly{Entity}` (e.g., `ReadOnlyMaterial`) in `.Abstractions` project
- **Mapping**: Extension methods in module project (abstractions don't have access to domain models)

#### Tests
- **File Structure**: Each use case has its own test file
- **Test Ordering**:
  1. `Seed()` - Initialize credentials and test data
  2. Authentication/Authorization tests
  3. Validation tests (all rules and edge cases)
  4. Success tests (happy path)
  5. `CleanUp()` - Delete database

### Creating New Features

1. **Create Feature Module**:
   ```
   PaintingProjectsManagement.Features.{FeatureName}/
   ```

2. **Create Abstractions**:
   ```
   PaintingProjectsManagement.Features.{FeatureName}.Abstractions/
   ```

3. **Create Tests**:
   ```
   PaintingProjectsManagement.Features.{FeatureName}.Tests/
   ```

4. **Register in API**:
   ```csharp
   app.Map{FeatureName}Feature();
   ```

### Code Organization Rules

1. **Models**: Place in `Models/` folder, inherit from `TenantEntity`
2. **Database Config**: Place in `Database/` folder, implement `IEntityTypeConfiguration<T>`
3. **Use Cases**: 
   - **Frontend Use Cases**: Place in `UseCases/` folder (CRUD operations, business logic)
   - **Integration Use Cases**: Place in `Integrations/` folder (queries only, for cross-module communication)
4. **DTOs**: 
   - **Frontend DTOs**: `{Entity}Header` or `{Entity}Details` (e.g., `MaterialDetails`)
   - **Integration DTOs**: `ReadOnly{Entity}` (e.g., `ReadOnlyMaterial`) - placed in `.Abstractions` project
5. **Mappers**: Place in `DataTransfer/Mappers/` folder as extension methods
6. **Integrations**: Place in `Integrations/` folder

### Naming Conventions

- **Classes**: PascalCase (e.g., `CreateMaterial`)
- **Methods**: PascalCase (e.g., `MapEndpoint`)
- **Properties**: PascalCase (e.g., `MaterialName`)
- **Files**: Match class names (e.g., `CreateMaterial.cs`)
- **Folders**: PascalCase (e.g., `UseCases/`, `DataTransfer/`)

### Endpoint Requirements

All endpoints must include:
```csharp
endpoints.MapPost("/api/materials", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
{
    var result = await dispatcher.SendAsync(request, cancellationToken);
    return ResultsMapper.FromResponse(result);
})
.Produces<MaterialDetails>(StatusCodes.Status200OK)  // Required
.RequireAuthorization()                              // Required (or AllowAnonymous)
.WithName("Create Material")                        // Required
.WithTags("Materials");                             // Required
```

### Validation Patterns

#### SmartValidator (for database entities)
```csharp
public class Validator : SmartValidator<Request, Material>
{
    public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
    {
    }

    protected override void ValidateBusinessRules()
    {
        // SmartValidator automatically validates DB constraints, PKs, and FKs
        // Focus only on business logic validation
        RuleFor(x => x.Name)
            .MustAsync(async (request, name, cancellationToken) =>
            {
                return !await Context.Set<Material>().AnyAsync(m => 
                    m.Name == name && m.TenantId == request.Identity.Tenant, cancellationToken);
            })
            .WithMessage(LocalizationService?.LocalizeString(MaterialsMessages.Create.MaterialWithNameAlreadyExists) ?? 
                        "A material with this name already exists.");
    }
}
```

#### AbstractValidator (for non-entity requests)
```csharp
public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date.");
    }
}
```

### Multi-Tenant Considerations

1. **Tenant Isolation**: All entities inherit from `TenantEntity`
2. **Tenant Filtering**: Always filter by `TenantId` in queries
3. **Tenant Validation**: Validate tenant context in all operations
4. **Cross-Tenant Communication**: Use abstractions for cross-module communication

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL (for development)
- Visual Studio 2022 or VS Code

### Running the Application

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd painting-projects-management
   ```

2. **Navigate to the backend**:
   ```bash
   cd back
   ```

3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

4. **Run the application**:
   ```bash
   dotnet run --project PaintingProjectsManagement.AppHost
   ```
   This uses .NET Aspire to start the API, PostgreSQL database and Blazor UI together.

5. **Access the services**:
   - API: `https://localhost:7001`
   - Swagger UI: `https://localhost:7001/swagger`
   - ReDoc: `https://localhost:7001/redoc`

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test PaintingProjectsManagement.Features.Materials.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName --project PaintingProjectsManagment.Database --startup-project PaintingProjectsManagement.Api

# Update database
dotnet ef database update --project PaintingProjectsManagment.Database --startup-project PaintingProjectsManagement.Api
```

## Contributing

1. Follow the established architecture patterns
2. Write comprehensive tests for all new features
3. Use abstractions for cross-module communication
4. Maintain tenant isolation in all operations
5. Follow the naming conventions and code organization rules
6. Update documentation when adding new features

## Architecture Benefits

- **Modularity**: Easy to add, remove, or modify features
- **Testability**: Comprehensive test coverage with isolated test environments
- **Scalability**: Clear separation allows for independent scaling
- **Maintainability**: Consistent patterns and clear structure
- **Security**: Built-in multi-tenant isolation
- **Flexibility**: Abstractions allow for easy integration and extension
