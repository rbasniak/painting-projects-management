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

        builder.Property(x => x.Category)
            .IsRequired();

        builder.ComplexProperty(x => x.PackageContent, owned =>
        {
            owned.Property(x => x.Amount)
                .HasColumnName("PackageContent_Amount")
                .IsRequired();
            owned.Property(x => x.Unit)
                .HasColumnName("PackageContent_Unit")
                .IsRequired();
        });

        builder.ComplexProperty(x => x.PackagePrice, owned =>
        {
            owned.Property(x => x.Amount)
                .HasColumnName("PackagePrice_Amount")
                .IsRequired();
            owned.Property(x => x.CurrencyCode)
                .HasColumnName("PackagePrice_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.Name, x.TenantId }).IsUnique();
    }
}
