using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectPictureConfig : IEntityTypeConfiguration<ProjectPicture>
{
    public void Configure(EntityTypeBuilder<ProjectPicture> builder)
    {
        builder.ToTable("ProjectPictures");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.ProjectId)
            .IsRequired();
            
        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(pp => pp.ProjectId);
    }
}