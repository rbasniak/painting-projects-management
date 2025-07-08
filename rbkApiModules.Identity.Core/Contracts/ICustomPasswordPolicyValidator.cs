using Microsoft.AspNetCore.Mvc;

namespace rbkApiModules.Identity.Core;

public interface ICustomPasswordPolicyValidator
{
    Task<ValidationProblemDetails> Validate(string password);
}