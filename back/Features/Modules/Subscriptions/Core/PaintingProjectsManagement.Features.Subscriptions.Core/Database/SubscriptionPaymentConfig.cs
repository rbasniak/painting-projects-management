namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class SubscriptionPaymentConfig : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.ToTable("payments", "subscriptions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(x => x.SubscriptionId).IsRequired();
        builder.Property(x => x.TierAtPayment).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ProviderTransactionId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.FailureReason).HasMaxLength(255);
        builder.Property(x => x.ProcessedUtc).IsRequired();
        builder.Property(x => x.CreatedUtc).IsRequired();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.SubscriptionId);
        builder.HasIndex(x => x.ProviderTransactionId);
    }
}
