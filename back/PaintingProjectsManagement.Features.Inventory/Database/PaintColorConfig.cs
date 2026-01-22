using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Inventory;

public class PaintColorConfig : IEntityTypeConfiguration<PaintColor>
{
    public void Configure(EntityTypeBuilder<PaintColor> builder)
    {
        builder.ToTable("paints_catalog.colors");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.HexColor)
            .IsRequired()
            .HasMaxLength(7);
            
        builder.Property(x => x.ManufacturerCode)
            .HasMaxLength(50); 
            
        builder.Property(x => x.Type)
            .IsRequired();
            
        builder.Property(x => x.LineId)
            .IsRequired();
            
        builder.HasOne(x => x.Line)
            .WithMany()
            .HasForeignKey(x => x.LineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.LineId);
        builder.HasIndex(x => x.Type);
        
        builder.HasIndex(p => new { p.LineId, p.Name })
            .IsUnique();

        builder.HasIndex(p => new { p.LineId, p.HexColor })
            .IsUnique();
    }
}