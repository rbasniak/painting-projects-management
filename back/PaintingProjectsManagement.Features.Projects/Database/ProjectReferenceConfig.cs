using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectReferenceConfig : IEntityTypeConfiguration<ProjectReference>
{
    public void Configure(EntityTypeBuilder<ProjectReference> builder)
    {
        builder.ToTable("ProjectReferences");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.ProjectId)
            .IsRequired();
            
        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(pr => pr.ProjectId);
    }
}