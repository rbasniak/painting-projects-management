using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
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
                        Debug.WriteLine($"***** Registering handler for {type.FullName.Split('.').Last().Split('+').First()}");
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