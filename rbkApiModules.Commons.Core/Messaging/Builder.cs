using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public static class Builder
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<Dispatcher>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.GetName().FullName.StartsWith("Microsoft"))
            .Where(x => !x.GetName().FullName.StartsWith("System"))
            .ToArray();

        RegisterQueryHandlers(services, typeof(IQueryHandler<,>), assemblies);
        RegisterQueryHandlers(services, typeof(ICommandHandler<,>), assemblies);
        RegisterQueryHandlers(services, typeof(INotificationHandler<>), assemblies);

        RegisterQueryHandlers(services, typeof(ICommandHandler<>), assemblies);

        RegisterValidators(services, assemblies);

        return services;
    }

    private static void RegisterQueryHandlers(IServiceCollection services, Type type, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            try
            {
                var queryHandlers = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces()
                        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == type));

                foreach (var handlerType in queryHandlers)
                {
                    var implementedInterfaces = handlerType.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == type);

                    foreach (var handlerInterface in implementedInterfaces)
                    {
                        services.AddScoped(handlerInterface, handlerType);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }
    }

    private static void RegisterValidators(IServiceCollection services, Assembly[] assemblies)
    {
        var type = typeof(AbstractValidator<>);

        foreach (var assembly in assemblies)
        {
            try
            {
                var validators = assembly.GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract)
                    .Where(x => x.BaseType != null && x.BaseType.IsGenericType)
                    .Where(x => x.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                    .Where(x => !x.Name.Contains("InlineValidator"));

                foreach (var validator in validators)
                {
                    services.AddScoped(validator.BaseType, validator);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }
    }
}