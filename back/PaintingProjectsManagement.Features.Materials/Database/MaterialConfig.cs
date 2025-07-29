using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Materials;

public class MaterialConfig : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.Unit)
            .IsRequired();
        
        builder.Property(x => x.PricePerUnit)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(m => new { m.Name, m.TenantId }).IsUnique();
    }
}