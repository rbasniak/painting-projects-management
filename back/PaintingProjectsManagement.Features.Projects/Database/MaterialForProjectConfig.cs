using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialForProjectConfig : IEntityTypeConfiguration<MaterialForProject>
{
    public void Configure(EntityTypeBuilder<MaterialForProject> builder)
    {
        builder.ToTable("ProjectMaterials");

        // Composite key
        builder.HasKey(e => new { e.ProjectId, e.MaterialId });
        
        builder.Property(x => x.ProjectId)
            .IsRequired();
            
        builder.Property(x => x.MaterialId)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.MaterialId);
    }
}