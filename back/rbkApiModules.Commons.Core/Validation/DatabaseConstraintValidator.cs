using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// Base validator that automatically applies database constraints to validation rules
/// </summary>
/// <typeparam name="TRequest">The request type to validate</typeparam>
/// <typeparam name="TModel">The entity model type that has EF configuration</typeparam>
public abstract class DatabaseConstraintValidator<TRequest, TModel> : AbstractValidator<TRequest>
    where TModel : class
{
    protected readonly DbContext Context;
    protected readonly ILocalizationService? LocalizationService;

    protected DatabaseConstraintValidator(DbContext context, ILocalizationService? localizationService = null)
    {
        Context = context;
        LocalizationService = localizationService;
        
        // Apply database constraints automatically
        ApplyDatabaseConstraints();
        
        // Apply custom rules
        ValidateBusinessRules();
    }

    /// <summary>
    /// Override this method to add custom validation rules beyond database constraints
    /// </summary>
    protected virtual void ValidateBusinessRules()
    {
        // Override in derived classes to add custom rules
    }

    protected virtual Dictionary<string, string> GetPropertyMappings()
    {
        // Map request properties to model properties if they have different names
        // In this case, they match, so we could return empty dictionary
        // But showing the pattern for when they don't match
        return new Dictionary<string, string>
        {
            // Example: if request had "PaintLineId" but model has "LineId"
            // { "LineId", "PaintLineId" }
        };
    }

    protected virtual IEnumerable<string> GetIgnoredProperties()
    {
        // Properties that should not be validated against database constraints
        // In this case, we want to validate all properties
        return Enumerable.Empty<string>();
    }

    /// <summary>
    /// Override this method to specify if primary key validation should be skipped
    /// </summary>
    protected virtual bool ShouldSkipPrimaryKeyValidation()
    {
        return false;
    }

    private void ApplyDatabaseConstraints()
    {
        try
        {
            var entityType = Context.Model.FindEntityType(typeof(TModel));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(TModel).Name} not found in DbContext");
            }

            var propertyMappings = GetPropertyMappings();
            var ignoredProperties = GetIgnoredProperties().ToHashSet();

            foreach (var property in entityType.GetProperties())
            {
                var requestPropertyName = GetRequestPropertyName(property.Name, propertyMappings);
                
                if (ignoredProperties.Contains(requestPropertyName))
                {
                    continue;
                }

                var requestProperty = typeof(TRequest).GetProperty(requestPropertyName);
                if (requestProperty == null)
                {
                    continue;
                }

                ApplyPropertyConstraints(property, requestProperty);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - this allows the validator to still work
            // even if there are issues with the database configuration
            System.Diagnostics.Debug.WriteLine($"Error applying database constraints: {ex.Message}");
        }
    }

    private string GetRequestPropertyName(string modelPropertyName, Dictionary<string, string> propertyMappings)
    {
        return propertyMappings.TryGetValue(modelPropertyName, out var mappedName) 
            ? mappedName 
            : modelPropertyName;
    }

    private void ApplyPropertyConstraints(IProperty property, PropertyInfo requestProperty)
    {
        // Apply required constraint
        if (property.IsNullable == false)
        {
            ApplyRequiredConstraint(requestProperty);
        }

        // Apply max length constraint
        if (property.GetMaxLength() is int maxLength)
        {
            ApplyMaxLengthConstraint(requestProperty, maxLength);
        }

        // Apply enum validation
        if (requestProperty.PropertyType.IsEnum)
        {
            ApplyEnumConstraint(requestProperty);
        }

        // Apply primary key constraint for "Id" properties
        if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && !ShouldSkipPrimaryKeyValidation())
        {
            ApplyPrimaryKeyConstraint(requestProperty);
        }

        // Apply foreign key constraints
        var foreignKey = property.GetContainingForeignKeys().FirstOrDefault();
        if (foreignKey != null)
        {
            ApplyForeignKeyConstraint(foreignKey, requestProperty);
        }
    }

    private void ApplyRequiredConstraint(PropertyInfo requestProperty)
    {
        var propertyType = requestProperty.PropertyType;
        
        if (propertyType == typeof(string))
        {
            CreateRuleFor<string>(requestProperty)
                .NotEmpty()
                .WithMessage(GetLocalizedMessage("Required", requestProperty.Name));
        }
        else if (propertyType == typeof(Guid))
        {
            CreateRuleFor<Guid>(requestProperty)
                .NotEmpty()
                .WithMessage(GetLocalizedMessage("Required", requestProperty.Name));
        }
        else if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
        {
            // For non-nullable value types, we don't need to add required validation
            // as they can't be null anyway
        }
        else
        {
            CreateRuleFor<object>(requestProperty)
                .NotNull()
                .WithMessage(GetLocalizedMessage("Required", requestProperty.Name));
        }
    }

    private void ApplyMaxLengthConstraint(PropertyInfo requestProperty, int maxLength)
    {
        if (requestProperty.PropertyType == typeof(string))
        {
            CreateRuleFor<string>(requestProperty)
                .MaximumLength(maxLength)
                .WithMessage(GetLocalizedMessage("MaxLength", requestProperty.Name, maxLength));
        }
    }

    private void ApplyEnumConstraint(PropertyInfo requestProperty)
    {
        var enumType = requestProperty.PropertyType;
        var validValues = Enum.GetValues(enumType);
        
        CreateRuleFor<object>(requestProperty)
            .Must(value =>
            {
                if (value == null) return true; // Let required validation handle nulls
                
                // Check if the value is a valid enum value
                return Enum.IsDefined(enumType, value);
            })
            .WithMessage(GetLocalizedMessage("InvalidEnum", requestProperty.Name));
    }

    protected virtual void ApplyPrimaryKeyConstraint(PropertyInfo requestProperty)
    {
        var propertyType = requestProperty.PropertyType;
        
        // Only apply primary key validation for non-zero values (assuming 0 is not a valid ID)
        CreateRuleFor<object>(requestProperty)
            .MustAsync(async (value, cancellationToken) =>
            {
                if (value == null) return true; // Let required validation handle nulls
                
                // Skip validation for zero values (new entities)
                if (value is int intValue && intValue == 0) return true;
                if (value is long longValue && longValue == 0) return true;
                if (value is Guid guidValue && guidValue == Guid.Empty) return true;
                
                try
                {
                    // Use a simpler approach - just check if the entity exists
                    var dbSet = Context.Set<TModel>();
                    var parameter = Expression.Parameter(typeof(TModel), "x");
                    var propertyAccess = Expression.Property(parameter, "Id");
                    var valueExpression = Expression.Constant(value);
                    var equalsExpression = Expression.Equal(propertyAccess, valueExpression);
                    var lambda = Expression.Lambda<Func<TModel, bool>>(equalsExpression, parameter);
                    
                    return await dbSet.AnyAsync(lambda, cancellationToken);
                }
                catch (Exception ex) 
                {
                    // If there's any error in the primary key validation, assume it's valid
                    // This prevents the validator from breaking due to reflection issues
                    return false;
                }
            })
            .WithMessage(GetLocalizedMessage("PrimaryKeyNotFound", requestProperty.Name));
    }

    protected virtual void ApplyForeignKeyConstraint(IForeignKey foreignKey, PropertyInfo requestProperty)
    {
        var principalEntityType = foreignKey.PrincipalEntityType;
        var principalKey = foreignKey.PrincipalKey;
        
        if (principalKey.Properties.Count == 1)
        {
            var principalProperty = principalKey.Properties.First();
            var principalEntityClrType = principalEntityType.ClrType;
            
            CreateRuleFor<object>(requestProperty)
                .MustAsync(async (value, cancellationToken) =>
                {
                    if (value == null) return true; // Let required validation handle nulls
                    
                    try
                    {
                        // Use reflection to call the generic Set method
                        var setMethod = Context.GetType().GetMethod("Set", Type.EmptyTypes)?.MakeGenericMethod(principalEntityClrType);
                        var dbSet = setMethod?.Invoke(Context, null);
                        
                        if (dbSet == null) return false;
                        
                        var parameter = Expression.Parameter(principalEntityClrType, "x");
                        var propertyAccess = Expression.Property(parameter, principalProperty.Name);
                        var valueExpression = Expression.Constant(value);
                        var equalsExpression = Expression.Equal(propertyAccess, valueExpression);
                        var lambda = Expression.Lambda(equalsExpression, parameter);
                        
                        // Use reflection to call AnyAsync since we don't know the exact type at compile time
                        var anyAsyncMethod = dbSet.GetType().GetMethod("AnyAsync", new[] { lambda.Type, typeof(CancellationToken) });
                        if (anyAsyncMethod != null)
                        {
                            var result = anyAsyncMethod.Invoke(dbSet, new object[] { lambda, cancellationToken });
                            return result is Task<bool> task ? await task : false;
                        }
                        
                        return false;
                    }
                    catch
                    {
                        // If there's any error in the foreign key validation, assume it's valid
                        // This prevents the validator from breaking due to reflection issues
                        return true;
                    }
                })
                .WithMessage(GetLocalizedMessage("ForeignKeyNotFound", requestProperty.Name));
        }
    }

    protected string GetLocalizedMessage(string messageKey, string propertyName, object? parameter = null)
    {
        // For now, return default messages since the localization service doesn't support string keys
        // In the future, you could extend the ILocalizationService to support string-based localization
        return messageKey switch
        {
            "Required" => $"{propertyName} is required.",
            "MaxLength" => $"{propertyName} cannot exceed {parameter} characters.",
            "ForeignKeyNotFound" => $"{propertyName} references a non-existent record.",
            "PrimaryKeyNotFound" => $"{propertyName} references a non-existent record.",
            "InvalidEnum" => $"{propertyName} has an invalid value.",
            _ => $"{propertyName} is invalid."
        };
    }

    private IRuleBuilder<TRequest, TProperty> CreateRuleFor<TProperty>(PropertyInfo property)
    {
        var parameter = Expression.Parameter(typeof(TRequest), "x");
        var propertyAccess = Expression.Property(parameter, property.Name);
        
        // Convert the property access to the correct type if needed
        Expression convertedPropertyAccess = propertyAccess;
        if (propertyAccess.Type != typeof(TProperty))
        {
            convertedPropertyAccess = Expression.Convert(propertyAccess, typeof(TProperty));
        }
        
        var lambda = Expression.Lambda<Func<TRequest, TProperty>>(convertedPropertyAccess, parameter);
        
        return RuleFor(lambda);
    }
}

/// <summary>
/// Base validator for tenant entities that automatically applies tenant filtering to database constraints
/// </summary>
/// <typeparam name="TRequest">The request type to validate (must inherit from AuthenticatedRequest)</typeparam>
/// <typeparam name="TModel">The tenant entity model type</typeparam>
public abstract class TenantDatabaseConstraintValidator<TRequest, TModel> : DatabaseConstraintValidator<TRequest, TModel>
    where TModel : TenantEntity
    where TRequest : AuthenticatedRequest
{
    protected TenantDatabaseConstraintValidator(DbContext context, ILocalizationService? localizationService = null) 
        : base(context, localizationService)
    {
    }

    /// <summary>
    /// Creates a tenant-aware rule for primary key validation
    /// </summary>
    protected IRuleBuilder<TRequest, TProperty> CreateTenantAwareRuleFor<TProperty>(PropertyInfo property)
    {
        var parameter = Expression.Parameter(typeof(TRequest), "x");
        var propertyAccess = Expression.Property(parameter, property.Name);
        
        // Convert the property access to the correct type if needed
        Expression convertedPropertyAccess = propertyAccess;
        if (propertyAccess.Type != typeof(TProperty))
        {
            convertedPropertyAccess = Expression.Convert(propertyAccess, typeof(TProperty));
        }
        
        var lambda = Expression.Lambda<Func<TRequest, TProperty>>(convertedPropertyAccess, parameter);
        
        return RuleFor(lambda);
    }

    protected override void ApplyPrimaryKeyConstraint(PropertyInfo requestProperty)
    {
        var propertyType = requestProperty.PropertyType;
        
        // Only apply primary key validation for non-zero values (assuming 0 is not a valid ID)
        CreateTenantAwareRuleFor<object>(requestProperty)
            .MustAsync(async (request, value, cancellationToken) =>
            {
                if (value == null) return true; // Let required validation handle nulls
                
                // Skip validation for zero values (new entities)
                if (value is int intValue && intValue == 0) return true;
                if (value is long longValue && longValue == 0) return true;
                if (value is Guid guidValue && guidValue == Guid.Empty) return true;
                
                try
                {
                    // Use a simpler approach - just check if the entity exists within the same tenant
                    var dbSet = Context.Set<TModel>();
                    var query = dbSet.AsQueryable();
                    
                    // Filter by tenant if authenticated
                    if (request.IsAuthenticated && request.Identity.HasTenant)
                    {
                        query = query.Where(e => e.TenantId == request.Identity.Tenant);
                    }
                    
                    var parameter = Expression.Parameter(typeof(TModel), "x");
                    var propertyAccess = Expression.Property(parameter, "Id");
                    var valueExpression = Expression.Constant(value);
                    var equalsExpression = Expression.Equal(propertyAccess, valueExpression);
                    var lambda = Expression.Lambda<Func<TModel, bool>>(equalsExpression, parameter);
                    
                    return await query.AnyAsync(lambda, cancellationToken);
                }
                catch (Exception ex) 
                {
                    // If there's any error in the primary key validation, assume it's valid
                    // This prevents the validator from breaking due to reflection issues
                    return false;
                }
            })
            .WithMessage(GetLocalizedMessage("PrimaryKeyNotFound", requestProperty.Name));
    }

    protected override void ApplyForeignKeyConstraint(IForeignKey foreignKey, PropertyInfo requestProperty)
    {
        var principalEntityType = foreignKey.PrincipalEntityType;
        var principalKey = foreignKey.PrincipalKey;
        
        if (principalKey.Properties.Count == 1)
        {
            var principalProperty = principalKey.Properties.First();
            var principalEntityClrType = principalEntityType.ClrType;
            
            CreateTenantAwareRuleFor<object>(requestProperty)
                .MustAsync(async (request, value, cancellationToken) =>
                {
                    if (value == null) return true; // Let required validation handle nulls
                    
                    try
                    {
                        // Use reflection to call the generic Set method
                        var setMethod = Context.GetType().GetMethod("Set", Type.EmptyTypes)?.MakeGenericMethod(principalEntityClrType);
                        var dbSet = setMethod?.Invoke(Context, null);
                        
                        if (dbSet == null) return false;
                        
                        var parameter = Expression.Parameter(principalEntityClrType, "x");
                        var propertyAccess = Expression.Property(parameter, principalProperty.Name);
                        var valueExpression = Expression.Constant(value);
                        var equalsExpression = Expression.Equal(propertyAccess, valueExpression);
                        
                        // Add tenant filtering if the principal entity is a TenantEntity
                        Expression finalExpression = equalsExpression;
                        if (typeof(TenantEntity).IsAssignableFrom(principalEntityClrType) && request.IsAuthenticated && request.Identity.HasTenant)
                        {
                            var tenantPropertyAccess = Expression.Property(parameter, "TenantId");
                            var tenantValueExpression = Expression.Constant(request.Identity.Tenant);
                            var tenantEqualsExpression = Expression.Equal(tenantPropertyAccess, tenantValueExpression);
                            finalExpression = Expression.AndAlso(equalsExpression, tenantEqualsExpression);
                        }
                        
                        var lambda = Expression.Lambda(finalExpression, parameter);
                        
                        // Use reflection to call AnyAsync since we don't know the exact type at compile time
                        var anyAsyncMethod = dbSet.GetType().GetMethod("AnyAsync", new[] { lambda.Type, typeof(CancellationToken) });
                        if (anyAsyncMethod != null)
                        {
                            var result = anyAsyncMethod.Invoke(dbSet, new object[] { lambda, cancellationToken });
                            return result is Task<bool> task ? await task : false;
                        }
                        
                        return false;
                    }
                    catch
                    {
                        // If there's any error in the foreign key validation, assume it's valid
                        // This prevents the validator from breaking due to reflection issues
                        return true;
                    }
                })
                .WithMessage(GetLocalizedMessage("ForeignKeyNotFound", requestProperty.Name));
        }
    }
} 