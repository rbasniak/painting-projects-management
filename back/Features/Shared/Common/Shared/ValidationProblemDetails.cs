using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;

/// <summary>
/// A <see cref="ProblemDetails"/> for validation errors.
/// </summary>
public class ValidationProblemDetails : ProblemDetails
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValidationProblemDetails"/>.
    /// </summary>
    public ValidationProblemDetails()
    {
        Title = "One or more validation errors occurred.";
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidationProblemDetails"/> using the specified <paramref name="errors"/>.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ValidationProblemDetails(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    /// <summary>
    /// Gets or sets the validation errors associated with this instance of <see cref="ValidationProblemDetails"/>.
    /// </summary>
    [JsonPropertyName("errors")]
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}