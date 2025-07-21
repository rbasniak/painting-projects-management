namespace PaintingProjectsManagement.Features.Models;

public class ModelCategoryConfig : IEntityTypeConfiguration<ModelCategory>
{
    public void Configure(EntityTypeBuilder<ModelCategory> builder)
    {
        builder.ToTable("ModelCategories");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => new { e.TenantId, e.Name})
            .IsUnique();
    }
}