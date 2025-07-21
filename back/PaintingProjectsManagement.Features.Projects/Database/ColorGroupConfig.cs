using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ColorGroupConfig : IEntityTypeConfiguration<ColorGroup>
{
    public void Configure(EntityTypeBuilder<ColorGroup> builder)
    {
        builder.ToTable("ProjectColorGroups");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.ProjectId)
            .IsRequired();
            
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(cg => cg.ProjectId);
        builder.HasIndex(cg => cg.Name);
    }
}