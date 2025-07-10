using Microsoft.Extensions.Logging;
using rbkApiModules.Commons.Core.Authentication;

namespace rbkApiModules.Identity.Core;
        
public class ConfirmUserEmail : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authentication/users/confirm-email", async (string email, string code, string tenant, 
            AuthEmailOptions authEmailOptions, Dispatcher dispatcher, ILogger<ConfirmUserEmail> logger, 
            CancellationToken cancellationToken) =>
        {
            try
            {
                await dispatcher.SendAsync(new Request { Email = email, ActivationCode = code, Tenant = tenant }, cancellationToken);
                
                return Results.Redirect(authEmailOptions.EmailData.ConfirmationSuccessUrl);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error confirming user {Email} for tenant {Tenant}", email, tenant);
                return Results.Redirect(authEmailOptions.EmailData.ConfirmationSuccessUrl);
            } 
        })
        .AllowAnonymous()
        .WithName("Confirm User Email")
        .WithTags("Authentication");
    }

    public class Request : ICommand
    {
        private string _tenant = string.Empty;

        public string Tenant
        {
            get => _tenant;
            set
            {
                _tenant = value?.ToUpper() ?? string.Empty;
            }
        }

        public string Email { get; set; } = string.Empty;
        public string ActivationCode { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _usersService;

        public Validator(IAuthService usersService, ILocalizationService localization)
        {
            _usersService = usersService;

            RuleFor(x => x.ActivationCode)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .MustAsync(BeValidPair)
                .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.InvalidActivationCode));
        }

        public async Task<bool> BeValidPair(Request request, string email, CancellationToken cancellationToken)
        {
            var user = await _usersService.FindUserAsync(email, request.Tenant, cancellationToken);

            if (user == null) return false;

            return user.ActivationCode == request.ActivationCode;
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
            var user = await _usersService.FindUserAsync(request.Email, request.Tenant, cancellationToken);

            await _usersService.ConfirmUserAsync(request.Email, request.Tenant, cancellationToken);
        }
    }
}