namespace PaintingProjectsManagement.Features.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ModelConfig : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.ToTable("Models");

        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.CategoryId)
            .IsRequired();
            
        builder.Property(e => e.Artist)
            .HasMaxLength(150);
            
        builder.Property(e => e.Tags)
            .HasConversion(
                v => string.Join(',', v.Order()),
                v => string.IsNullOrEmpty(v) ? Array.Empty<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.Name);
    }
}