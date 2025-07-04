using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Paints;

public class BrandLineConfig : IEntityTypeConfiguration<PaintLine>
{
    public void Configure(EntityTypeBuilder<PaintLine> builder)
    {
        builder.ToTable("PaintLines");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.BrandId)
            .IsRequired();
            
        builder.HasOne(e => e.Brand)
            .WithMany()
            .HasForeignKey(e => e.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.Name);
        builder.HasIndex(l => l.BrandId);
    }
}