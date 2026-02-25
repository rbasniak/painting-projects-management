using Microsoft.Extensions.DependencyInjection;

namespace PaintingProjectsManagement.Features.Models;

public static class Builder
{
    public static IServiceCollection AddModelsFeature(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventHandler<ModelCreated>, ModelCreatedHandler>();
        services.AddScoped<IDomainEventHandler<ModelDeleted>, ModelDeletedHandler>();
        services.AddScoped<IDomainEventHandler<ModelDetailsChanged>, ModelUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<ModelCoverPictureChanged>, ModelUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<ModelPicturesChanged>, ModelUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<ModelRated>, ModelUpdatedHandler>();
        services.AddScoped<IDomainEventHandler<ModelMustHaveChanged>, ModelUpdatedHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapPrintingModelsFeature(this IEndpointRouteBuilder app)
    {
        return UseCasesBuilder.MapPrintingModelsFeature((IEndpointRouteBuilder)app);
    }
}
