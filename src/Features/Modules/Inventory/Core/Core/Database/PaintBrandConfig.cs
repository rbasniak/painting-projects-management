using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintBrandConfig : IEntityTypeConfiguration<PaintBrand>
{
    public void Configure(EntityTypeBuilder<PaintBrand> builder)
    {
        builder.ToTable("paints_catalog.brands");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
           .IsUnique();
    }
}