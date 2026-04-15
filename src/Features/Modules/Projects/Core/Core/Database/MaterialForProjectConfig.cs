using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialForProjectConfig : IEntityTypeConfiguration<MaterialForProject>
{
    public void Configure(EntityTypeBuilder<MaterialForProject> builder)
    {
        builder.ToTable("projects.project_materials");

        // Composite key
        builder.HasKey(x => new { x.ProjectId, x.MaterialId });
        
        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.MaterialId)
            .IsRequired();

        builder.ComplexProperty(x => x.Quantity, owned =>
        {
            owned.Property(x => x.Value)
                .HasColumnName("Quantity_Value")
                .IsRequired();

            owned.Property(x => x.Unit)
                .HasColumnName("Quantity_Unit")
                .IsRequired();
        });

        // Foreign key relationships
        builder.HasOne<Project>()
            .WithMany(x => x.Materials)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.MaterialId);
    }
}
