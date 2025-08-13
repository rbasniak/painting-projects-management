using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// EF configuration for <see cref="IntegrationDelivery"/>.
/// </summary>
public class IntegrationDeliveryConfig : IEntityTypeConfiguration<IntegrationDelivery>
{
    public void Configure(EntityTypeBuilder<IntegrationDelivery> builder)
    {
        builder.ToTable("IntegrationDeliveries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Subscriber).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Attempts).IsRequired();
        builder.Property(x => x.ProcessedUtc);

        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.ProcessedUtc);
    }
}
