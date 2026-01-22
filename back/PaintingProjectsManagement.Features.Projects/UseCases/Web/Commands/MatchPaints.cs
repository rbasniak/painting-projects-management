using System.Text.Json;
using PaintingProjectsManagement.Features.Inventory.Integration;

namespace PaintingProjectsManagement.Features.Projects;

public class MatchPaints : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/projects/{projectId}/color-sections/match-paints", async (Guid projectId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var request = new Request { ProjectId = projectId };
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Match Paints")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ProjectId { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ProjectId)
                .MustAsync(async (request, projectId, cancellationToken) =>
                    await Context.Set<Project>().AnyAsync(
                        p => p.Id == projectId && p.TenantId == request.Identity.Tenant,
                        cancellationToken))
                .WithMessage("Project not found.");
        }
    }

    public class Handler(DbContext _context, IDispatcher _dispatcher) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            // Load project with all color groups and sections
            var project = await _context.Set<Project>()
                .Include(p => p.ColorGroups)
                    .ThenInclude(g => g.Sections)
                .FirstAsync(p => p.Id == request.ProjectId &&
                               p.TenantId == request.Identity.Tenant,
                         cancellationToken);

            // For each color section, find matches
            foreach (var group in project.ColorGroups)
            {
                foreach (var section in group.Sections)
                {
                    // Create command request that implements IFindColorMatchesCommand
                    var findMatchesCommand = new FindColorMatchesCommandRequest
                    {
                        ReferenceColor = section.ReferenceColor,
                        MaxResults = 10
                    };
                    // Note: Identity should be automatically set by the framework when the request
                    // goes through IDispatcher, based on the current authenticated user context

                    // Use typed dispatcher - doesn't return object, returns typed response
                    var matchesResponse = await _dispatcher.SendAsync<FindColorMatchesCommandRequest, IReadOnlyCollection<ColorMatchResult>>(
                        findMatchesCommand,
                        cancellationToken);

                    if (matchesResponse.IsSuccess && matchesResponse.Data != null)
                    {
                        // Serialize matches to JSON with camelCase property names
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var json = JsonSerializer.Serialize(matchesResponse.Data, options);
                        section.UpdateSuggestedColors(json);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }

    // Command request class that implements the integration interface
    public class FindColorMatchesCommandRequest : AuthenticatedRequest, ICommand, IFindColorMatchesCommand
    {
        public string ReferenceColor { get; set; } = string.Empty;
        public int MaxResults { get; set; } = 10;

        // Explicit interface implementation
        string IFindColorMatchesCommand.ReferenceColor => ReferenceColor;
        int IFindColorMatchesCommand.MaxResults => MaxResults;
        string IFindColorMatchesCommand.Tenant => Identity.Tenant ?? string.Empty;
    }
}
