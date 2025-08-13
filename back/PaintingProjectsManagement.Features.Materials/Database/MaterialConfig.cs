using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Materials;

public class MaterialConfig : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("materials.materials");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.OwnsOne(x => x.PackageContent, owned =>
        {
            owned.Property(p => p.Amount)
                .HasColumnName("PackageContent_Amount")
                .IsRequired();
            owned.Property(p => p.Unit)
                .HasColumnName("PackageContent_Unit")
                .IsRequired();
        });
        
        builder.OwnsOne(x => x.PackagePrice, owned =>
        {
            owned.Property(p => p.Amount)
                .HasColumnName("PackagePrice_Amount")
                .IsRequired();
            owned.Property(p => p.CurrencyCode)
                .HasColumnName("PackagePrice_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(m => new { m.Name, m.TenantId }).IsUnique();
    }
}