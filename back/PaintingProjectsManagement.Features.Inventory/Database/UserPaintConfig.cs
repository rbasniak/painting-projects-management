using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaintingProjectsManagement.Features.Inventory;

public class UserPaintConfig : IEntityTypeConfiguration<UserPaint>
{
    public void Configure(EntityTypeBuilder<UserPaint> builder)
    {
        builder.ToTable("user_paints", "inventory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PaintColorId)
            .IsRequired();

        builder.HasOne(x => x.PaintColor)
            .WithMany()
            .HasForeignKey(x => x.PaintColorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Username);
        builder.HasIndex(x => x.PaintColorId);
        builder.HasIndex(x => new { x.Username, x.PaintColorId })
            .IsUnique();
    }
}
