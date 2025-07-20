namespace PaintingProjectsManagement.Features.Models;

internal class PrioritizeModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/models/prioritize", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .RequireAuthorization()
        .WithName("Prioritize Models")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid[] ModelIds { get; set; } = Array.Empty<Guid>();
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(DbContext context)
        {
            RuleFor(x => x.ModelIds)
                .NotNull()
                .Must(ids => ids.Length == ids.Distinct().Count())
                .WithMessage("Model IDs must not contain duplicates.");

            RuleForEach(x => x.ModelIds)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) =>
                    await context.Set<Model>().AnyAsync(m => m.Id == id && m.Score == 5, cancellationToken))
                .WithMessage("All models must exist and have a score of 5.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            // Reset all model priorities to the default value (-1)
            var allModels = await _context.Set<Model>().ToListAsync(cancellationToken);
            foreach (var model in allModels)
            {
                model.ResetPriority();
            }

            // Set new priorities for the models in the provided order
            // Starting from highest priority (array length) down to 1
            var prioritizedModels = allModels.Where(x => request.ModelIds.Contains(x.Id)).ToArray();

            var modelDict = prioritizedModels.ToDictionary(m => m.Id);

            // Set priorities based on position in the array (reverse order)
            // First element in the array gets the highest priority number
            for (int i = 0; i < request.ModelIds.Length; i++)
            {
                if (modelDict.TryGetValue(request.ModelIds[i], out var model))
                {
                    // Priority is based on position (highest number = highest priority)
                    var priority = request.ModelIds.Length - i;
                    model.ResetPriority(priority);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}