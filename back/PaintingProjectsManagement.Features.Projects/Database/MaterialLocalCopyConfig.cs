using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialLocalCopyConfig : IEntityTypeConfiguration<MaterialLocalCopy>
{
    public void Configure(EntityTypeBuilder<MaterialLocalCopy> builder)
    {
        builder.ToTable("projects.materials_local_copy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PricePerUnit).IsRequired();
        builder.Property(x => x.UpdatedUtc).IsRequired();
    }
}
