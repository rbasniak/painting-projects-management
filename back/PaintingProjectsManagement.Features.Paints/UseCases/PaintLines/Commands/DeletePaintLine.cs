namespace PaintingProjectsManagement.Features.Paints;

internal class DeletePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/paints/lines/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Delete Brand Line")
        .WithTags("Paint Lines");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : SmartValidator<Request, PaintLine>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // Check that there are no paint colors associated with this line
            RuleFor(x => x.Id).MustAsync(async (id, cancellationToken) => 
                !await Context.Set<PaintColor>().AnyAsync(x => x.LineId == id, cancellationToken))
                .WithMessage("Cannot delete a paint line that has associated paint colors. Remove the paint colors first.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var paintLine = await _context.Set<PaintLine>().FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            _context.Remove(paintLine);
            
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}