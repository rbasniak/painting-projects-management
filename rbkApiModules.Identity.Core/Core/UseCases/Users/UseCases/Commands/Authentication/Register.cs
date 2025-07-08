using FluentValidation;
using rbkApiModules.Identity.Core.DataTransfer.Users;
namespace rbkApiModules.Identity.Core;

public class Register : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/authentication/register", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok();
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithTags("Users");
    }

    public class Request : ICommand<UserDetails>, IUserMetadata
    {
        public string Tenant { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordConfirmation { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = [];
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _usersService;
        private readonly ITenantsService _tenantsService;

        public Validator(IAuthService usersService, ILocalizationService localization, IEnumerable<ICustomPasswordPolicyValidator> passwordValidators,
            IEnumerable<ICustomUserMetadataValidator> metadataValidators, ITenantsService tenantsService)
        {
            _usersService = usersService;
            _tenantsService = tenantsService;

            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync(UserDoesNotExistOnDatabase)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.UserAlreadyExists));

            RuleFor(x => x.Tenant)
                .NotEmpty()
                .MustAsync(TenantExistInDatabase)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.TenantNotFound));

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(EmailDoesNotExistOnDatabase)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.EmailAlreadyUsed));

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(255);

            RuleFor(x => x)
                .UserMetadataIsValid(metadataValidators, localization);

            RuleFor(x => x.Password)
                .NotEmpty()
                .Must(PasswordsBeTheSame)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.PasswordsMustBeTheSame))
                .PasswordPoliciesAreValid(passwordValidators, localization);
        }

        private async Task<bool> EmailDoesNotExistOnDatabase(Request request, string email, CancellationToken cancellationToken)
        {
            var isRegistered = await _usersService.IsUserRegisteredAsync(email, request.Tenant, cancellationToken);

            return !isRegistered;
        }

        private bool PasswordsBeTheSame(Request request, string _)
        {
            return request.PasswordConfirmation == request.Password;
        }

        private async Task<bool> TenantExistInDatabase(Request request, string tenantAlias, CancellationToken cancellationToken)
        {
            var tenant = await _tenantsService.FindAsync(tenantAlias, cancellationToken);

            return tenant != null;
        }

        private async Task<bool> UserDoesNotExistOnDatabase(Request request, string username, CancellationToken cancellationToken)
        {
            var user = await _usersService.FindUserAsync(username, request.Tenant, cancellationToken);

            return user == null;
        }
    }

    public class Handler : ICommandHandler<Request, UserDetails>
    {
        private readonly IAuthService _usersService;
        private readonly IAuthenticationMailService _mailingService;

        public Handler(IAuthService usersService, IAuthenticationMailService mailingService)
        {
            _usersService = usersService;
            _mailingService = mailingService;
        }

        public async Task<UserDetails> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var user = await _usersService.CreateUserAsync(request.Tenant, request.Username, request.Password, request.Email, request.DisplayName,
                AvatarGenerator.GenerateBase64Avatar(request.DisplayName), false, AuthenticationMode.Credentials, request.Metadata, cancellationToken);

            _mailingService.SendConfirmationMail(user.DisplayName, user.Email, user.ActivationCode);

            return UserDetails.FromModel(user);
        }
    }
}
