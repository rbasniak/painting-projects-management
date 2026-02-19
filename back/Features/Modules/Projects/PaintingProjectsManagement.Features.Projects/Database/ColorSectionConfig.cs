using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ColorSectionConfig : IEntityTypeConfiguration<ColorSection>
{
    public void Configure(EntityTypeBuilder<ColorSection> builder)
    {
        builder.ToTable("project.project_color_sections");

        builder.HasKey(x => x.Id);
         
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.ReferenceColor)
            .IsRequired()
            .HasMaxLength(7); // Hex color format
            
        builder.Property(x => x.Zone)
            .IsRequired();
            
        builder.Property(x => x.ColorGroupId)
            .IsRequired();

        // Store suggested colors as JSONB column
        builder.Property(x => x.SuggestedColorsJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("[]")
            .IsRequired();

        // Relationship with ColorGroup
        builder.HasOne(x => x.ColorGroup)
            .WithMany()
            .HasForeignKey(x => x.ColorGroupId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cs => new { cs.ColorGroupId, cs.Zone })
            .IsUnique(); 
    }
}