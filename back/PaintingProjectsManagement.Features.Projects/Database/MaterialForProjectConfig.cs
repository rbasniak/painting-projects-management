using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialForProjectConfig : IEntityTypeConfiguration<MaterialForProject>
{
    public void Configure(EntityTypeBuilder<MaterialForProject> builder)
    {
        builder.ToTable("ProjectMaterials");

        // Composite key
        builder.HasKey(e => new { e.ProjectId, e.MaterialId });
        
        builder.Property(e => e.ProjectId)
            .IsRequired();
            
        builder.Property(e => e.MaterialId)
            .IsRequired();

        // Indexes
        builder.HasIndex(pm => pm.ProjectId);
        builder.HasIndex(pm => pm.MaterialId);
    }
}