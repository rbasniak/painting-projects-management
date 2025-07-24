namespace PaintingProjectsManagement.Features.Paints;

public class UpdatePaintLine : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/paints/lines", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintLineDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(Claims.MANAGE_PAINTS)
        .WithName("Update Brand Line")
        .WithTags("Paint Lines");
    }

    public class Request : ICommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, PaintLine>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            // Check for unique name constraint within the same brand (excluding current line)
            RuleFor(x => x).MustAsync(async (request, cancellationToken) =>
            {
                // First get the line being updated to determine its brand
                var currentLine = await Context.Set<PaintLine>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (currentLine == null)
                {
                    return true; // ID validation will be handled separately
                }
                
                // Check if another line with the same name exists in the same brand
                return !await Context.Set<PaintLine>().AnyAsync(x => 
                    x.Name == request.Name && 
                    x.BrandId == currentLine.BrandId && 
                    x.Id != request.Id, 
                    cancellationToken);
            })
                .WithMessage("Another paint line with this name already exists for this brand.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var line = await _context.Set<PaintLine>()
                .Include(x => x.Brand)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);
            
            line.UpdateDetails(request.Name);
            
            await _context.SaveChangesAsync(cancellationToken);

            var result = PaintLineDetails.FromModel(line); 
            
            return CommandResponse.Success(result);
        }
    }
}