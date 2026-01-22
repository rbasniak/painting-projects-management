namespace PaintingProjectsManagement.Features.Inventory;

public class RemoveFromMyPaints : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/inventory/my-paints/{paintColorId}", async (Guid paintColorId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var request = new Request { PaintColorId = paintColorId };
            try
            {
                var result = await dispatcher.SendAsync(request, cancellationToken);
                return ResultsMapper.FromResponse(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization()
        .WithName("Remove From My Paints")
        .WithTags("Inventory");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid PaintColorId { get; set; }
    }

    public class Validator : SmartValidator<Request, UserPaint>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization) { }

        protected override void ValidateBusinessRules() { }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var username = request.Identity.Tenant ?? string.Empty;

            var userPaint = await _context.Set<UserPaint>()
                .FirstOrDefaultAsync(up => up.Username == username && up.PaintColorId == request.PaintColorId, cancellationToken);

            if (userPaint == null)
                throw new KeyNotFoundException("User paint not found.");

            _context.Set<UserPaint>().Remove(userPaint);
            await _context.SaveChangesAsync(cancellationToken);
            return CommandResponse.Success();
        }
    }
}
