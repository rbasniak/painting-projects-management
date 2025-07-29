using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectPictureConfig : IEntityTypeConfiguration<ProjectPicture>
{
    public void Configure(EntityTypeBuilder<ProjectPicture> builder)
    {
        builder.ToTable("ProjectPictures");

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