using Microsoft.Extensions.DependencyInjection;
using PaintingProjectsManagement.Features.Models;

namespace Microsoft.Extensions.DependencyInjection;

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
        app.MapModelCategoriesFeature();
        app.MapModelsFeature();

        return app;
    }
} 