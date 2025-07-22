using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Paints;

public class BrandConfig : IEntityTypeConfiguration<PaintBrand>
{
    public void Configure(EntityTypeBuilder<PaintBrand> builder)
    {
        builder.ToTable("PaintBrands");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Name)
           .IsUnique();
    }
}