using Microsoft.AspNetCore.Mvc;

namespace rbkApiModules.Identity.Core;

public interface ICustomUserMetadataValidator
{
    Task<ValidationProblemDetails> Validate(Dictionary<string, string> userMetadata);
}