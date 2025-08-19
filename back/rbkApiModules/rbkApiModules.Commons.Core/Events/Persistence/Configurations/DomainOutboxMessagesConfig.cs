// TODO: DONE, REVIEWED

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace rbkApiModules.Commons.Core;

public class DomainOutboxMessagesConfig : IEntityTypeConfiguration<DomainOutboxMessages>
{
    public void Configure(EntityTypeBuilder<DomainOutboxMessages> builder)
    {
        builder.ToTable("DomainOutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.TenantId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OccurredUtc).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(50);
        builder.Property(x => x.CausationId).HasMaxLength(50);
        // In production scenario we probably want to use a JSONB column for the payload
        builder.Property(x => x.Payload).IsRequired(); 
        builder.Property(x => x.CreatedUtc).IsRequired();
        builder.Property(x => x.ProcessedUtc);
        builder.Property(x => x.Attempts).IsRequired();
        builder.Property(x => x.DoNotProcessBeforeUtc);

        // Indices to speed up queries
        builder.HasIndex(x => x.ProcessedUtc);

        builder.HasIndex(x => x.DoNotProcessBeforeUtc);

        // Query-time filters: ProcessedUtc IS NULL, (DoNotProcessBeforeUtc IS NULL OR <= now),
        // (ClaimedUntilUtc IS NULL OR < now), Attempts < @maxAttempts, ORDER BY CreatedUtc LIMIT @batch

        // Primary partial index to drive ORDER BY + LIMIT
        builder.HasIndex(x => x.CreatedUtc)
            .HasFilter($@"""{nameof(DomainOutboxMessages.ProcessedUtc)}"" IS NULL");

        // Help when many messages are delayed via backoff
        builder.HasIndex(x => x.DoNotProcessBeforeUtc)
            .HasFilter($@"""{nameof(DomainOutboxMessages.ProcessedUtc)}"" IS NULL AND ""{nameof(DomainOutboxMessages.DoNotProcessBeforeUtc)}"" IS NOT NULL");
    }
} 