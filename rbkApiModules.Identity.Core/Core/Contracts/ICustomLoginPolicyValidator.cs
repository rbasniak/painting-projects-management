using Microsoft.AspNetCore.Mvc;

namespace rbkApiModules.Identity.Core;

public interface ICustomLoginPolicyValidator
{
    Task<ValidationProblemDetails> Validate(string tenant, string username, string password);
}

