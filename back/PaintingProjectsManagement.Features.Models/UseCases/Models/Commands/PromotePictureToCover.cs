namespace PaintingProjectsManagement.Features.Models;

public class PromotePictureToCover : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/models/{modelId}/promote-picture", async (Guid modelId, Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            request.ModelId = modelId;
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Promote Picture To Cover")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ModelId { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.PictureUrl)
                .NotEmpty()
                .WithMessage("Picture URL is required.");

            RuleFor(x => x)
                .MustAsync(async (request, cancellation) =>
                {
                    var model = await Context.Set<Model>().FirstOrDefaultAsync(x => x.Id == request.ModelId, cancellation);
                    return model != null && model.Pictures.Contains(request.PictureUrl);
                })
                .WithMessage("The specified picture URL must exist in the model's pictures collection.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var model = await _context.Set<Model>()
                .Include(x => x.Category)
                .FirstAsync(x => x.Id == request.ModelId, cancellationToken);

            model.UpdateCoverPicture(request.PictureUrl);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return CommandResponse.Success();
        }
    }
}
