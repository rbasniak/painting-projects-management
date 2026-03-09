using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PaintingProjectsManagement.UI.Modules.Shared;

public class HttpErrorHandler : DelegatingHandler
{
    private static readonly HashSet<string> PublicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/login",
        "/signin"
    };

    private readonly NavigationManager _navigation;
    private readonly ISnackbar _snackbar;
    private readonly ProblemDetailsState _problemState;

    public HttpErrorHandler(NavigationManager navigation, ISnackbar snackbar, ProblemDetailsState problemState)
    {
        _navigation = navigation;
        _snackbar = snackbar;
        _problemState = problemState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                var currentPath = new Uri(_navigation.Uri).AbsolutePath;
                if (!PublicRoutes.Contains(currentPath))
                {
                    _navigation.NavigateTo("/signin", true);
                }
                break;
            case HttpStatusCode.BadRequest:
                var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: cancellationToken);
                if (validation?.Errors != null)
                {
                    foreach (var pair in validation.Errors)
                    {
                        foreach (var err in pair.Value)
                        {
                            _snackbar.Add(err, Severity.Error);
                        }
                    }
                }
                break;
            case HttpStatusCode.InternalServerError:
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);
                _problemState.Details = problem;
#if DEBUG
                _navigation.NavigateTo("/error", true);
#else
                _navigation.NavigateTo("/debug", true);
#endif
                break;
        }

        return response;
    }
}
