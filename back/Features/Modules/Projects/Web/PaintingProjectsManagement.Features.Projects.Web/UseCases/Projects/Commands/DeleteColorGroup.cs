namespace PaintingProjectsManagement.Features.Projects;

public class DeleteColorGroup : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/projects/color-groups", async (Guid colorGroupId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { ColorGroupId = colorGroupId }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Delete Color Group")
        .WithTags("Projects");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid ColorGroupId { get; set; }
    }

    public class Validator : SmartValidator<Request, Project>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.ColorGroupId)
                .MustAsync(async (request, colorGroupId, cancellationToken) =>
                    await Context.Set<ColorGroup>()
                        .AnyAsync(x => x.Id == colorGroupId && x.Project.TenantId == request.Identity.Tenant, cancellationToken))
                .WithMessage("Color group not found.");

            RuleFor(x => x)
                .MustAsync((request, cancellationToken) =>
                    ArchivedProjectValidation.IsEditableByColorGroupAsync(Context, request.Identity.Tenant, request.ColorGroupId, cancellationToken))
                .WithMessage(ArchivedProjectValidation.ReadOnlyMessage);
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var colorGroup = await _context.Set<ColorGroup>()
                .Include(x => x.Project)
                .FirstAsync(x => x.Id == request.ColorGroupId, cancellationToken); 

            colorGroup.Project.RemoveColorGroup(request.ColorGroupId);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
