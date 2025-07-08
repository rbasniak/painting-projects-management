namespace rbkApiModules.Identity.Core;

public class CreateClaim : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/authorization/claims", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthenticationClaims.MANAGE_CLAIMS)
        .WithName("Create Claim")
        .WithTags("Claims");
    }

    public class Request : ICommand<ClaimDetails>
    {
        public required string Identification { get; set; }
        public required string Description { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IClaimsService _claimsService;

        public Validator(IClaimsService claimsService, ILocalizationService localization)
        {
            _claimsService = claimsService;

            RuleFor(x => x.Identification)
                .NotEmpty()
                .MustAsync(NotExistsInDatabaseWithSameIdentification)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.ClaimIdentificationAlreadyUsed));

            RuleFor(x => x.Description)
                .NotEmpty()
                .MustAsync(NotExistsInDatabaseWithSameDescription)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.ClaimDescriptionAlreadyUsed));
        }

        private async Task<bool> NotExistsInDatabaseWithSameIdentification(Request request, string identification, CancellationToken cancellationToken)
        {
            return (await _claimsService.FindByIdentificationAsync(identification, cancellationToken)) == null;
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

        public async Task<ClaimDetails> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var claim = new Claim(request.Identification, request.Description);

            claim = await _claimsService.CreateAsync(claim, cancellationToken);

            return ClaimDetails.FromModel(claim);
        }
    }
}