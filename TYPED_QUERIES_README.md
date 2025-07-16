# Typed Query System for Module-to-Module Communication

This document describes the new typed query system that provides type-safe communication between modules.

## Overview

The existing mediator system uses `object` for query results, which requires casting and makes module-to-module communication error-prone. The new typed query system provides compile-time type safety.

## Components

### 1. Typed Query Interface
```csharp
public interface IQuery<TResponse> : IRequest<QueryResponse<TResponse>> { }
```

### 2. Typed Response
```csharp
public sealed class QueryResponse<T> : BaseResponse
{
    public static QueryResponse<T> Success(T result) { ... }
    public static QueryResponse<T> Failure(ProblemDetails problem) { ... }
    public T? GetResult() { ... }
}
```

### 3. Typed Handler Interface
```csharp
public interface ITypedQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, QueryResponse<TResponse>>
    where TQuery : IQuery<TResponse>
{ }
```

## Usage Examples

### Creating a Typed Query

**In the Abstractions project:**
```csharp
namespace MyModule.Abstractions;

public static partial class GetMyData
{
    public sealed class Request : IQuery<IReadOnlyCollection<MyData>>
    {
        public Guid[] Ids { get; set; } = Array.Empty<Guid>();
    }
}
```

### Implementing the Handler

**In the main module:**
```csharp
namespace MyModule.Integrations;

public static partial class GetMyData
{
    public sealed class Handler(DbContext _context) : ITypedQueryHandler<Abstractions.GetMyData.Request, IReadOnlyCollection<MyData>>
    {
        public async Task<QueryResponse<IReadOnlyCollection<MyData>>> HandleAsync(Abstractions.GetMyData.Request request, CancellationToken cancellationToken)
        {
            var data = await _context.Set<MyEntity>()
                .Where(x => request.Ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var results = data.Select(x => x.MapToDto()).AsReadOnly();
            return QueryResponse<IReadOnlyCollection<MyData>>.Success(results);
        }
    }
}
```

### Using the Typed Query

**In another module:**
```csharp
public class MyHandler(DbContext _context, IDispatcher _dispatcher) : IQueryHandler<Request>
{
    public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var queryRequest = new GetMyData.Request
        {
            Ids = request.SomeIds
        };

        var response = await _dispatcher.SendAsync(queryRequest, cancellationToken);
        
        // Type-safe access to the result - no casting needed!
        var data = response.GetResult();
        
        // Use the data...
        return QueryResponse.Success(result);
    }
}
```

## Benefits

1. **Type Safety**: Compile-time checking prevents runtime casting errors
2. **IntelliSense**: Full IDE support with autocomplete and type information
3. **Refactoring Safety**: Renaming types will update all usages automatically
4. **Clear Contracts**: The return type is explicitly defined in the query interface
5. **No Casting**: Eliminates the need for unsafe type casting

## Migration from Untyped Queries

### Before (Untyped):
```csharp
public sealed class Request : IQuery
{
    public Guid[] Ids { get; set; } = Array.Empty<Guid>();
}

// Usage with casting:
var response = await _dispatcher.SendAsync(request, cancellationToken);
var data = ((IReadOnlyCollection<MyData>)response.Data).Select(...);
```

### After (Typed):
```csharp
public sealed class Request : IQuery<IReadOnlyCollection<MyData>>
{
    public Guid[] Ids { get; set; } = Array.Empty<Guid>();
}

// Usage without casting:
var response = await _dispatcher.SendAsync(request, cancellationToken);
var data = response.GetResult().Select(...);
```

## Registration

Typed query handlers are automatically registered by the existing `AddMessaging()` extension method. No additional configuration is required.

## When to Use

- **Use typed queries** for module-to-module communication where you need type safety
- **Use untyped queries** for HTTP endpoints where the response will be serialized to JSON

## Example: GetMaterialsForProject

The `GetMaterialsForProject` query has been migrated to use the typed system:

```csharp
// Abstractions
public sealed class Request : IQuery<IReadOnlyCollection<ReadOnlyMaterial>>
{
    public Guid[] MaterialIds { get; set; } = Array.Empty<Guid>();
}

// Handler
public sealed class Handler(DbContext _context) : ITypedQueryHandler<Request, IReadOnlyCollection<ReadOnlyMaterial>>
{
    public async Task<QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Implementation...
        return QueryResponse<IReadOnlyCollection<ReadOnlyMaterial>>.Success(results);
    }
}

// Usage
var response = await _dispatcher.SendAsync(materialsRequest, cancellationToken);
var materialDetails = response.GetResult().Select(MaterialDetails.FromReadOnlyMaterial).ToArray();
``` 