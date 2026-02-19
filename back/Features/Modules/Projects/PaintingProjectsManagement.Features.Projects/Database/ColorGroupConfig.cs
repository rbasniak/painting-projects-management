using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ColorGroupConfig : IEntityTypeConfiguration<ColorGroup>
{
    public void Configure(EntityTypeBuilder<ColorGroup> builder)
    {
        builder.ToTable("projects.project_color_groups");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.ProjectId)
            .IsRequired();
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(cg => new { cg.ProjectId, cg.Name })
            .IsUnique();
    }
}