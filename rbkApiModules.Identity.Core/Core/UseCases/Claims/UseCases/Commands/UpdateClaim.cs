namespace rbkApiModules.Identity.Core;

public class UpdateClaim : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/authorization/claims", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthenticationClaims.MANAGE_CLAIMS)
        .WithName("Update Claim")
        .WithTags("Claims");
    }

    public class Request : ICommand<ClaimDetails>
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IClaimsService _claimsService;

        public Validator(IClaimsService claimsService, ILocalizationService localization)
        {
            _claimsService = claimsService;

            RuleFor(x => x.Id)
                .ClaimExistOnDatabase(claimsService, localization);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MustAsync(NotExistsInDatabaseWithSameDescription)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.ClaimDescriptionAlreadyUsed));
        }

        private async Task<bool> NotExistsInDatabaseWithSameDescription(Request request, string identification, CancellationToken cancellationToken)
        {
            return (await _claimsService.FindByDescriptionAsync(identification, cancellationToken)) == null;
        }
    }

    public class Handler : ICommandHandler<Request, ClaimDetails>
    {
        private readonly IClaimsService _claimsService;

        public Handler(IClaimsService claimsService)
        {
            _claimsService = claimsService;
        }

        public async Task<CommandResponse<ClaimDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            await _claimsService.RenameAsync(request.Id, request.Description, cancellationToken);

            var claim = await _claimsService.FindAsync(request.Id, cancellationToken);

            return CommandResponse.Success(ClaimDetails.FromModel(claim));
        }
    }
}