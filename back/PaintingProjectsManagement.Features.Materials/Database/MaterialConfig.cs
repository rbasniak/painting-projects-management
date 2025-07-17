using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Materials;

public class MaterialConfig : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Unit)
            .IsRequired();
        
        builder.Property(e => e.PricePerUnit)
            .IsRequired();

        builder.HasIndex(m => m.Name);
    }
}