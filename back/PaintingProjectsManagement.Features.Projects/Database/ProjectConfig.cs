using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.PictureUrl)
            .HasMaxLength(255);
            
        builder.Property(e => e.StartDate)
            .IsRequired();
            
        builder.Property(e => e.EndDate);

        // One-to-one relationship with ProjectSteps
        builder.HasOne(e => e.Steps)
            .WithOne()
            .HasForeignKey<ProjectSteps>(e => e.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationships with collections
        builder.HasMany(e => e.Materials)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.References)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.Pictures)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.Sections)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.StartDate);
        builder.HasIndex(p => p.EndDate);
    }
}