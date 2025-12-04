using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ReadOnlyMaterialConfig : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("projects.projections.materials");
        builder.HasKey(x => new { x.Tenant, x.Id });
        builder.Property(x => x.Tenant).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(50);
        builder.Property(x => x.UpdatedUtc).IsRequired();

        builder.ComplexProperty(x => x.PricePerUnit, x =>
        {
            x.Property(x => x.Amount).HasColumnName("PricePerUnit_Amount").IsRequired();
            x.Property(m => m.Currency).HasColumnName("PricePerUnit_Currency").IsRequired().HasMaxLength(3);
        });
    }
}
