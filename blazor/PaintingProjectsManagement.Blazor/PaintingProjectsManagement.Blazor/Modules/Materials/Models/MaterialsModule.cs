using PaintingProjectsManagement.Blazor.Modules.Shared;

namespace PaintingProjectsManagement.Blazor.Modules.Materials;

public class MaterialsModule : IModule
{
    public string Name => "Materials";
    public string Route => "materials";
    public string Icon => "icon";
    public int Order => 1;
}

public static class Builder
{
    public static IServiceCollection AddMaterialsModule(this IServiceCollection services)
    {
        services.AddScoped<IMaterialsService, MaterialsService>();

        return services;
    }
}