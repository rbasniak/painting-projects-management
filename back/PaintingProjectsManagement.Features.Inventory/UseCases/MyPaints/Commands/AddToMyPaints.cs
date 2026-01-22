namespace PaintingProjectsManagement.Features.Inventory;

public class AddToMyPaints : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/inventory/my-paints", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Add To My Paints")
        .WithTags("Inventory");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public IReadOnlyList<Guid> PaintColorIds { get; set; } = Array.Empty<Guid>();
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

            var existing = await _context.Set<UserPaint>()
                .Where(up => up.Username == username && request.PaintColorIds.Contains(up.PaintColorId))
                .Select(up => up.PaintColorId)
                .ToListAsync(cancellationToken);

            var toAdd = request.PaintColorIds
                .Where(id => !existing.Contains(id))
                .Distinct()
                .ToList();

            var existingPaints = await _context.Set<PaintColor>()
                .Where(p => toAdd.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            foreach (var id in toAdd)
            {
                if (!existingPaints.Contains(id))
                    continue;

                _context.Set<UserPaint>().Add(new UserPaint(username, id));
            }

            await _context.SaveChangesAsync(cancellationToken);
            return CommandResponse.Success();
        }
    }
}
