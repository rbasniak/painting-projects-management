using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectStepsConfig : IEntityTypeConfiguration<ProjectSteps>
{
    public void Configure(EntityTypeBuilder<ProjectSteps> builder)
    {
        builder.ToTable("ProjectSteps");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        builder.Property(e => e.ProjectId)
            .IsRequired();

        // One-to-one relationships with step data
        builder.OwnsOne(e => e.Planning, planning => 
        {
            planning.WithOwner().HasForeignKey("StepId");
            planning.Property(p => p.Id).ValueGeneratedOnAdd();
            planning.Property(p => p.Date);
            planning.Property(p => p.Duration).IsRequired();
        });
        
        builder.OwnsOne(e => e.Painting, painting => 
        {
            painting.WithOwner().HasForeignKey("StepId");
            painting.Property(p => p.Id).ValueGeneratedOnAdd();
            painting.Property(p => p.Date);
            painting.Property(p => p.Duration).IsRequired();
        });
        
        builder.OwnsOne(e => e.Preparation, preparation => 
        {
            preparation.WithOwner().HasForeignKey("StepId");
            preparation.Property(p => p.Id).ValueGeneratedOnAdd();
            preparation.Property(p => p.Date);
            preparation.Property(p => p.Duration).IsRequired();
        });
        
        builder.OwnsOne(e => e.Supporting, supporting => 
        {
            supporting.WithOwner().HasForeignKey("StepId");
            supporting.Property(p => p.Id).ValueGeneratedOnAdd();
            supporting.Property(p => p.Date);
            supporting.Property(p => p.Duration).IsRequired();
        });

        // Index
        builder.HasIndex(ps => ps.ProjectId);
    }
}