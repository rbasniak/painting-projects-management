using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Paints;

public class PaintBrandConfig : IEntityTypeConfiguration<PaintBrand>
{
    public void Configure(EntityTypeBuilder<PaintBrand> builder)
    {
        builder.ToTable("PaintBrands");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
           .IsUnique();
    }
}