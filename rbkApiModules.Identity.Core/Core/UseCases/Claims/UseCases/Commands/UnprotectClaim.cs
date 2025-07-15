namespace rbkApiModules.Identity.Core;

public class UnprotectClaim : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authorization/claims/unprotect", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthenticationClaims.CHANGE_CLAIM_PROTECTION)
        .WithName("Unprotect Claim")
        .WithTags("Claims");
    }

    public class Request : ICommand<ClaimDetails>
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator(IClaimsService claimsService, ILocalizationService localization)
        {
            RuleFor(x => x.Id)
                .ClaimExistOnDatabase(claimsService, localization);
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
            await _claimsService.UnprotectAsync(request.Id, cancellationToken);

            var claim = await _claimsService.FindAsync(request.Id, cancellationToken);

            return CommandResponse.Success(ClaimDetails.FromModel(claim));
        }
    }
}