using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace rbkApiModules.Commons.Core;

/// <summary>
/// EF configuration for <see cref="OutboxIntegrationEvent"/>.
/// </summary>
public class OutboxIntegrationEventConfig : IEntityTypeConfiguration<OutboxIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<OutboxIntegrationEvent> builder)
    {
        builder.ToTable("OutboxIntegrationEvents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OccurredUtc).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.CausationId).HasMaxLength(100);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.CreatedUtc).IsRequired();
        builder.Property(x => x.ProcessedUtc);
        builder.Property(x => x.Attempts).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Name, x.Version });
        builder.HasIndex(x => x.ProcessedUtc);
        builder.HasIndex(x => x.CreatedUtc);
    }
}
