using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class ReadOnlyMaterialConfig : IEntityTypeConfiguration<ReadOnlyMaterial>
{
    public void Configure(EntityTypeBuilder<ReadOnlyMaterial> builder)
    {
        builder.ToTable("ReadOnlyMaterials");
        builder.HasKey(x => new { x.Tenant, x.Id });
        builder.Property(x => x.Tenant).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PricePerUnit).IsRequired();
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(50);
        builder.Property(x => x.UpdatedUtc).IsRequired();
    }
}
