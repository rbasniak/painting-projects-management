using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintLineConfig : IEntityTypeConfiguration<PaintLine>
{
    public void Configure(EntityTypeBuilder<PaintLine> builder)
    {
        builder.ToTable("paints_catalog.lines"); 

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.BrandId)
            .IsRequired();
            
        builder.HasOne(x => x.Brand)
            .WithMany()
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.BrandId, e.Name })
           .IsUnique();
    }
}