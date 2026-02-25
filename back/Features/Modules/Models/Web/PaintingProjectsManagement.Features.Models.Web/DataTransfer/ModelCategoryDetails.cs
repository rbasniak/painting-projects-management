namespace PaintingProjectsManagement.Features.Models;

public class ModelCategoryDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public static ModelCategoryDetails FromModel(ModelCategory category)
    {
        return new ModelCategoryDetails
        {
            Id = category.Id,
            Name = category.Name
        };
    }
}
