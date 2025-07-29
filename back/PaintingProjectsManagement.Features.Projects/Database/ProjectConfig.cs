using Microsoft.EntityFrameworkCore.Metadata.Builders;
using rbkApiModules.Commons.Relational;

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

        builder.OwnsOne(e => e.Steps, steps =>
        {
            steps.Property(s => s.Planning).HasConversion(new JsonValueConverter<ProjectStepData[]>());
            steps.Property(s => s.Painting).HasConversion(new JsonValueConverter<ProjectStepData[]>());
            steps.Property(s => s.Preparation).HasConversion(new JsonValueConverter<ProjectStepData[]>());
            steps.Property(s => s.Supporting).HasConversion(new JsonValueConverter<ProjectStepData[]>());
        });

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
            
        builder.HasMany(e => e.Groups)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.StartDate);
        builder.HasIndex(p => p.EndDate);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();
    }
}