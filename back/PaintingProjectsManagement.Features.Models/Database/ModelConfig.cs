namespace PaintingProjectsManagement.Features.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ModelConfig : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.ToTable("Models");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.CategoryId)
            .IsRequired();
            
        builder.Property(x => x.Artist)
            .HasMaxLength(50);

        builder.Property(x => x.Franchise)
            .HasMaxLength(75);

        builder.Property(x => x.MustHave)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.Name })
           .IsUnique();
    }
}