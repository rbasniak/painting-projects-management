namespace PaintingProjectsManagement.Features.Subscriptions;

public sealed class TenantSubscriptionConfig : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("subscriptions", "subscriptions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Tier).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.LastPaymentTransactionId).HasMaxLength(128);
        builder.Property(x => x.CreatedUtc).IsRequired();
        builder.Property(x => x.UpdatedUtc).IsRequired();

        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.CurrentPeriodEndUtc });
    }
}
