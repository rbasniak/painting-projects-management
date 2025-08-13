using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace rbkApiModules.Commons.Core;

public class IntegrationDeliveryConfig : IEntityTypeConfiguration<IntegrationDelivery>
{
    public void Configure(EntityTypeBuilder<IntegrationDelivery> builder)
    {
        builder.ToTable("IntegrationDeliveries");
        builder.HasKey(x => new { x.EventId, x.Subscriber });

        builder.Property(x => x.Subscriber).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Attempts).IsRequired();
        builder.Property(x => x.DoNotProcessBeforeUtc);
        builder.Property(x => x.ProcessedUtc);

        builder.HasIndex(x => x.ProcessedUtc);
    }
}
