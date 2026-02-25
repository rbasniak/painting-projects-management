namespace PaintingProjectsManagement.Features.Models;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ModelCategoryConfig : IEntityTypeConfiguration<ModelCategory>
{
    public void Configure(EntityTypeBuilder<ModelCategory> builder)
    {
        builder.ToTable("models.categories");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.TenantId, x.Name})
            .IsUnique();
    }
}
