using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialCopyConfig : IEntityTypeConfiguration<MaterialCopy>
{
    public void Configure(EntityTypeBuilder<MaterialCopy> builder)
    {
        builder.ToTable("MaterialCopies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PricePerUnit).IsRequired();
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(50);
        builder.Property(x => x.UpdatedUtc).IsRequired();
    }
}
