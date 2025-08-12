using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectReferenceConfig : IEntityTypeConfiguration<ProjectReference>
{
    public void Configure(EntityTypeBuilder<ProjectReference> builder)
    {
        builder.ToTable("projects.picture_references");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.ProjectId)
            .IsRequired();
            
        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(x => x.ProjectId);
    }
}