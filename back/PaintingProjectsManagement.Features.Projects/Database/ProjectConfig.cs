using Microsoft.EntityFrameworkCore.Metadata.Builders;
using rbkApiModules.Commons.Relational;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

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

        builder.OwnsOne(x => x.Steps, steps =>
        {
            steps.Property(x => x.Planning).HasConversion(new JsonValueConverter<ProjectStepData[]>());
            steps.Property(x => x.Painting).HasConversion(new JsonValueConverter<ProjectStepData[]>());
            steps.Property(x => x.Preparation).HasConversion(new JsonValueConverter<ProjectStepData[]>());
            steps.Property(x => x.Supporting).HasConversion(new JsonValueConverter<ProjectStepData[]>());
        });

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
            
        builder.HasMany(x => x.Groups)
            .WithOne()
            .HasForeignKey(x => x.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => x.EndDate);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique();
    }
}