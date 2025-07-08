using rbkApiModules.Identity.Core;

internal static class ClaimsValidators
{
    public static IRuleBuilderOptions<T, Guid> ClaimExistOnDatabase<T>(this IRuleBuilder<T, Guid> rule, IClaimsService claims, ILocalizationService localization)
    {
        return rule
            .MustAsync(async (claimId, cancellationToken) =>
            {
                return await claims.FindAsync(claimId, cancellationToken) != null;
            })
            .WithMessage(command => localization.LocalizeString(AuthenticationMessages.Validations.ClaimNotFound));
    }
}

