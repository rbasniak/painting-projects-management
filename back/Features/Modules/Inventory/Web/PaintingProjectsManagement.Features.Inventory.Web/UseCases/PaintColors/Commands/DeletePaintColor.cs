namespace PaintingProjectsManagement.Features.Inventory;

public class DeletePaintColor : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/paints/colors/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Delete Paint Color")
        .WithTags("Paint Colors");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : SmartValidator<Request, PaintColor>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintColor = await _context.Set<PaintColor>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            _context.Remove(paintColor);
            
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}