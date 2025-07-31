using System.Reflection;

namespace PaintingProjectsManagement.UI.Client.Modules;

public class ModuleRegistrationService
{
    private readonly List<IModule> _modules = new();

    public IReadOnlyCollection<IModule> Modules => _modules.AsReadOnly();

    public void RegisterModule(IModule module)
    {
        if (!_modules.Any(m => m.GetType() == module.GetType()))
        {
            _modules.Add(module);
        }
    }

    public void RegisterModulesFromAssembly(Assembly assembly)
    {
        var moduleTypes = assembly.GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var moduleType in moduleTypes)
        {
            if (Activator.CreateInstance(moduleType) is IModule module)
            {
                RegisterModule(module);
            }
        }
    }

    public IEnumerable<IModule> GetOrderedModules()
    {
        return _modules.OrderBy(m => m.Order);
    }
} 