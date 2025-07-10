namespace rbkApiModules.Identity.Core;

public class DeleteUser : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authentication/users/delete", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok();
        })
        .RequireAuthorization(AuthenticationClaims.MANAGE_USERS)
        .WithName("Delete User")
        .WithTags("Authentication");
    }

    public class Request : AuthenticatedRequest, ICommand 
    {
        public string Username { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _usersService;

        public Validator(IAuthService usersService, ILocalizationService localization)
        {
            _usersService = usersService;

            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync(UserExistsOnDatabase)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.UserNotFound))
                .Must(UserCannotDeleteItself)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.UserCannotDeleteItselft));
        } 

        private bool UserCannotDeleteItself(Request request, string username)
        {
            return username.ToLower() != request.Identity.Username.ToLower();
        }

        private async Task<bool> UserExistsOnDatabase(Request request, string username, CancellationToken cancellationToken)
        {
            var user = await _usersService.FindUserAsync(username, request.Identity.Tenant, cancellationToken);
            var result = user != null;
            return result;
        }
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly IAuthService _usersService;
        private readonly ILocalizationService _localization;

        public Handler(IAuthService usersService, ILocalizationService localization)
        {
            _usersService = usersService;
            _localization = localization;
        }

        public async Task HandleAsync(Request request, CancellationToken cancellationToken)
        {
            try
            {
                await _usersService.DeleteUserAsync(request.Identity.Tenant, request.Username, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ExpectedInternalException(_localization.LocalizeString(AuthenticationMessages.Erros.CannotDeleteUser), ex);
            }
        }
    }
}
