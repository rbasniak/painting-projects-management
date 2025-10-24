namespace PaintingProjectsManagement.Features.Models;

public class DeleteModelPicture : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/models/picture/delete", async (Request data,  IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(data, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Model Picture")
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

    public class Handler(DbContext _context, IFileStorage _fileStorage) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var model = await _context.Set<Model>()
                .FirstAsync(x => x.Id == request.ModelId, cancellationToken);

            if (!string.IsNullOrEmpty(request.PictureUrl))
            {
                await _fileStorage.DeleteFileAsync(request.PictureUrl, cancellationToken);
            }

            model.RemovePicture(request.PictureUrl);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
