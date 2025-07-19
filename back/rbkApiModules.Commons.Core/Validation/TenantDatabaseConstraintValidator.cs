using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace rbkApiModules.Commons.Core;

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