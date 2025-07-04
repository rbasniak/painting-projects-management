namespace PaintingProjectsManagement.Features.Paints;

public class PaintLineDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string? BrandName { get; set; }

    public static PaintLineDetails FromModel(PaintLine line)
    {
        return new PaintLineDetails
        {
            Id = line.Id,
            Name = line.Name,
            BrandId = line.BrandId,
            BrandName = line.Brand?.Name
        };
    }
}