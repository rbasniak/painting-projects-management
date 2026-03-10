namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed record PaymentChargeRequest
{
    public required string TenantId { get; init; }
    public required decimal AmountUsd { get; init; }
    public required string Description { get; init; }
}

public sealed record PaymentChargeResult
{
    public required bool Succeeded { get; init; }
    public required string Provider { get; init; }
    public required string TransactionId { get; init; }
    public string FailureReason { get; init; } = string.Empty;
}

public interface IPaymentGateway
{
    Task<PaymentChargeResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken);
}

public sealed class DummyPaymentGateway : IPaymentGateway
{
    public Task<PaymentChargeResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken)
    {
        var transactionId = $"dummy-{request.TenantId.ToLowerInvariant()}-{Guid.NewGuid():N}";

        return Task.FromResult(new PaymentChargeResult
        {
            Succeeded = true,
            Provider = "dummy",
            TransactionId = transactionId
        });
    }
}
