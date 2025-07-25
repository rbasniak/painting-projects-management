using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ColorSectionConfig : IEntityTypeConfiguration<ColorSection>
{
    public void Configure(EntityTypeBuilder<ColorSection> builder)
    {
        builder.ToTable("ProjectColorSections");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Color)
            .IsRequired()
            .HasMaxLength(7); // Hex color format
            
        builder.Property(e => e.Zone)
            .IsRequired();
            
        builder.Property(e => e.ColorGroupId)
            .IsRequired();

        // Store array of Guids as comma-separated string
        builder.Property(e => e.SuggestedColorIds)
            .HasConversion(
                v => string.Join(',', v),
                v => string.IsNullOrEmpty(v) ? Array.Empty<Guid>() : 
                    v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToArray());

        // Relationship with ColorGroup
        builder.HasOne(e => e.ColorGroup)
            .WithMany(g => g.Sections)
            .HasForeignKey(e => e.ColorGroupId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cs => new { cs.ColorGroupId, cs.Zone })
            .IsUnique(); 
    }
}