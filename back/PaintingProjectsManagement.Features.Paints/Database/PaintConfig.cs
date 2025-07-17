using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Paints;

public class PaintConfig : IEntityTypeConfiguration<PaintColor>
{
    public void Configure(EntityTypeBuilder<PaintColor> builder)
    {
        builder.ToTable("Paints");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.HexColor)
            .IsRequired()
            .HasMaxLength(7);
            
        builder.Property(e => e.ManufacturerCode)
            .HasMaxLength(50);
            
        builder.Property(e => e.BottleSize)
            .IsRequired();
            
        builder.Property(e => e.Price)
            .IsRequired();
            
        builder.Property(e => e.Type)
            .IsRequired();
            
        builder.Property(e => e.LineId)
            .IsRequired();
            
        builder.HasOne(e => e.Line)
            .WithMany()
            .HasForeignKey(e => e.LineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.LineId);
        builder.HasIndex(p => p.Type);
    }
}