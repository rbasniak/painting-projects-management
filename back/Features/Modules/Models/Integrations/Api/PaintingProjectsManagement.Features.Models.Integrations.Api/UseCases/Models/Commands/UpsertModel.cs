namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

using System.Text.Json.Serialization;

public class UpsertModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/integrations/models", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces<PaintingProjectsManagement.Features.Models.ModelDetails>(StatusCodes.Status200OK)
        .RequireAuthorization(ModelsIntegrationsApiAuthentication.PolicyName)
        .WithName("Integrations - Upsert Model")
        .WithTags("Models Integrations");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        [JsonPropertyName("id")]
        public string? InternalId { get; set; }
        public Guid CategoryId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string[] Characters { get; set; } = Array.Empty<string>();
        public string Name { get; set; } = string.Empty;
        public BaseSize BaseSize { get; set; }
        public FigureSize FigureSize { get; set; }
        public int NumberOfFigures { get; set; }
        public string Franchise { get; set; } = string.Empty;
        public ModelType Type { get; set; } = ModelType.Unknown;
        public int SizeInMb { get; set; }
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.InternalId)
                .MaximumLength(512)
                .WithMessage("Id cannot exceed 512 characters.");

            RuleForEach(x => x.Tags)
                .NotEmpty()
                .WithMessage("Each tag cannot be empty")
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage("Each tag cannot be whitespace")
                .MaximumLength(25)
                .WithMessage("Each tag cannot exceed 25 characters");

            RuleForEach(x => x.Characters)
                .NotEmpty()
                .WithMessage("Each character cannot be empty")
                .Must(character => !string.IsNullOrWhiteSpace(character))
                .WithMessage("Each character cannot be whitespace")
                .MaximumLength(50)
                .WithMessage("Each character cannot exceed 50 characters");

            RuleFor(x => x.NumberOfFigures)
                .GreaterThan(0)
                .WithMessage("NumberOfFigures must be greater than zero");

            RuleFor(x => x.SizeInMb)
                .GreaterThanOrEqualTo(0)
                .WithMessage("SizeInMb must be greater than or equal to zero");
        }
    }

    public class Handler(DbContext context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var tenant = request.Identity.Tenant;
            var normalizedId = NormalizeId(request.InternalId);

            var category = await context.Set<ModelCategory>()
                .Where(x => x.TenantId == tenant)
                .FirstAsync(x => x.Id == request.CategoryId, cancellationToken);

            Model? model = null;
            if (!string.IsNullOrWhiteSpace(normalizedId))
            {
                model = await context.Set<Model>()
                    .Include(x => x.Category)
                    .FirstOrDefaultAsync(
                        x => x.TenantId == tenant && x.Identity == normalizedId,
                        cancellationToken);
            }

            if (model is null)
            {
                model = new Model(
                    tenant,
                    request.Name,
                    category,
                    request.Characters ?? Array.Empty<string>(),
                    request.Franchise,
                    request.Type,
                    request.Artist,
                    request.Tags ?? Array.Empty<string>(),
                    request.BaseSize,
                    request.FigureSize,
                    request.NumberOfFigures,
                    request.SizeInMb,
                    normalizedId);

                await context.AddAsync(model, cancellationToken);
            }
            else
            {
                model.UpdateDetails(
                    request.Name,
                    category,
                    request.Characters ?? Array.Empty<string>(),
                    request.Artist,
                    request.Tags ?? Array.Empty<string>(),
                    request.BaseSize,
                    request.FigureSize,
                    request.NumberOfFigures,
                    request.Franchise,
                    request.Type,
                    request.SizeInMb,
                    normalizedId);
            }

            await context.SaveChangesAsync(cancellationToken);
            return CommandResponse.Success(PaintingProjectsManagement.Features.Models.ModelDetails.FromModel(model));
        }

        private static string? NormalizeId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return id.Trim();
        }
    }
}
