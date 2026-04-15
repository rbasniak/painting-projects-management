namespace PaintingProjectsManagement.Features.Models;

public sealed class GetPublicModelsOwnerKey : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models/public/owner-key", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<PublicModelsOwnerKey>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Get Public Models Owner Key")
        .WithTags("Models");
    }

    public sealed class Request : AuthenticatedRequest, IQuery
    {
    }

    public sealed class Validator : AbstractValidator<Request>
    {
    }

    public sealed class Handler : IQueryHandler<Request>
    {
        public Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var ownerKey = PublicModelsOwnerKeyCodec.Encode(request.Identity.Tenant);

            return Task.FromResult(QueryResponse.Success(new PublicModelsOwnerKey
            {
                OwnerKey = ownerKey
            }));
        }
    }
}
