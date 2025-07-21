using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace rbkApiModules.Commons.Core;

public static class Builder
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.GetName().FullName.StartsWith("Microsoft"))
            .Where(x => !x.GetName().FullName.StartsWith("System"))
            .ToArray();

        RegisterHandlers(services, typeof(IRequestHandler<,>), assemblies);
        RegisterHandlers(services, typeof(INotificationHandler<>), assemblies);

        RegisterValidators(services, assemblies);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Type handlerOpenGenericType, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract).ToArray();

                foreach (var type in types)
                { 
                    var interfaces = type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerOpenGenericType)
                        .ToArray();

                    foreach (var @interface in interfaces)
                    {
                        // Debug.WriteLine($"***** Registering handler for {type.FullName.Split('.').Last().Split('+').First()}");
                        services.AddScoped(@interface, type);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Optionally log
                continue;
            }
        }
    }

    private static void RegisterValidators(IServiceCollection services, Assembly[] assemblies)
    {
        var validatorType = typeof(AbstractValidator<>);

        foreach (var assembly in assemblies)
        {
            try
            {
                var validators = assembly.GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract)
                    .Where(x => InheritsFromAbstractValidator(x))
                    .Where(x => !x.Name.Contains("InlineValidator"))
                    .Where(x => !x.Name.Contains("ChildRulesContainer"))
                    .ToList();

                foreach (var validator in validators)
                {
                    var serviceType = GetValidatorInterface(validator);
                    if (serviceType != null)
                    {
                        services.AddScoped(serviceType, validator);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }
    }

    private static bool InheritsFromAbstractValidator(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                return true;

            type = type.BaseType;
        }

        return false;
    }

    private static Type GetValidatorInterface(Type validatorType)
    {
        // Find the AbstractValidator<T> type it implements
        while (validatorType != null && validatorType != typeof(object))
        {
            if (validatorType.IsGenericType &&
                validatorType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return validatorType;
            }

            validatorType = validatorType.BaseType;
        }

        return null;
    }

}