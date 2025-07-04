namespace PaintingProjectsManagement.Features.Models;

public class ModelCategory
{
    // Constructor for EF Core, do not remote it or make it public
    private ModelCategory() { }

    public ModelCategory(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public void UpdateDetails(string name)
    {
        Name = name;
    } 
}
