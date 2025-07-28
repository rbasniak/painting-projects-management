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
    public string Name { get; private set; } = string.Empty;
    public MaterialUnit Unit { get; private set; }
    public double PricePerUnit { get; private set; }

    public void UpdateDetails(string name, MaterialUnit unit, double pricePerUnit)
    {
        Name = name;
        Unit = unit;
        PricePerUnit = pricePerUnit;
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
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
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
        // Validation logic
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        // Business logic
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
                .Where(m => m.TenantId == request.Identity.Tenant)
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
        // Setup test data
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Material()
    {
        // Test authentication
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Create_Material_When_Name_Is_Empty()
    {
        // Test validation
    }

    [Test, NotInParallel(Order = 14)]
    public async Task User_Can_Create_Material()
    {
        // Test success scenario
    }
}
```

### Testing Guidelines

1. **Test Ordering**: Use `NotInParallel` and `Order` attributes for test sequencing
2. **Database Isolation**: Each test class uses a separate database instance
3. **Authentication Testing**: Test both authenticated and unauthenticated scenarios
4. **Validation Testing**: Test all validation rules and edge cases
5. **Business Logic Testing**: Verify business rules and constraints
6. **Database Verification**: Always verify database state after operations

### Test Categories

- **Authentication Tests**: Verify proper authentication and authorization
- **Validation Tests**: Test input validation and business rules
- **Success Tests**: Verify correct behavior for valid inputs
- **Integration Tests**: Test cross-module communication
- **Edge Case Tests**: Test boundary conditions and error scenarios

## Development Guidelines

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
3. **Use Cases**: Place in `UseCases/` folder, separate Commands and Queries
4. **DTOs**: Place in `DataTransfer/` folder
5. **Mappers**: Place in `DataTransfer/Mappers/` folder
6. **Integrations**: Place in `Integrations/` folder

### Naming Conventions

- **Classes**: PascalCase (e.g., `CreateMaterial`)
- **Methods**: PascalCase (e.g., `MapEndpoint`)
- **Properties**: PascalCase (e.g., `MaterialName`)
- **Files**: Match class names (e.g., `CreateMaterial.cs`)
- **Folders**: PascalCase (e.g., `UseCases/`, `DataTransfer/`)

### Validation Patterns

```csharp
public class Validator : SmartValidator<Request, Material>
{
    public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
    {
    }

    protected override void ValidateBusinessRules()
    {
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

### Multi-Tenant Considerations

1. **Tenant Isolation**: All entities inherit from `TenantEntity`
2. **Tenant Filtering**: Always filter by `TenantId` in queries
3. **Tenant Validation**: Validate tenant context in all operations
4. **Cross-Tenant Communication**: Use abstractions for cross-module communication

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQLite (for development)
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
   dotnet run --project PaintingProjectsManagement.Api
   ```

5. **Access the API**:
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
