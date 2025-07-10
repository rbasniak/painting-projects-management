namespace rbkApiModules.Identity.Core;

public class ChangePassword : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authentication/change-password", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("Change Password")
        .WithTags("Authentication");
        }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string PasswordConfirmation { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _usersService;

        public Validator(IAuthService usersService, ILocalizationService localization, IEnumerable<ICustomPasswordPolicyValidator> passwordValidators)
        {
            _usersService = usersService;

            RuleFor(x => x.OldPassword)
                .MustAsync(MatchPassword)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.OldPasswordDoesNotMatch));

            RuleFor(x => x.NewPassword)
                    .NotEmpty()
                .Must(PasswordsBeTheSame)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.PasswordsMustBeTheSame))
                .PasswordPoliciesAreValid(passwordValidators, localization);
        }

        private bool PasswordsBeTheSame(Request request, string _)
        {
            return request.PasswordConfirmation == request.NewPassword;
        }

        public async Task<bool> MatchPassword(Request request, string password, CancellationToken cancellationToken)
        {
            var user = await _usersService.FindUserAsync(request.Identity.Username, request.Identity.Tenant, cancellationToken);

            return PasswordHasher.VerifyPassword(password, user.Password);
        }
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly IAuthService _usersService;

        public Handler(IAuthService usersService)
        {
            _usersService = usersService;
        }

        public async Task HandleAsync(Request request, CancellationToken cancellationToken)
        {
            await _usersService.ChangePasswordAsync(request.Identity.Tenant, request.Identity.Username, request.NewPassword, cancellationToken);
        }
    }
}
