using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects.projects");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.PictureUrl)
            .HasMaxLength(255);
            
        builder.Property(x => x.StartDate)
            .IsRequired();
            
        builder.Property(x => x.EndDate);

        // One-to-one relationship with ProjectSteps
        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationships with collections
        builder.HasMany(x => x.Materials)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(x => x.References)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(x => x.Pictures)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => x.EndDate);
    }
}