using PaintingProjectsManagement.Features.Projects.UI;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public record ProjectHeader
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; 
}

public class ProjectDetails
{
    private static readonly UrlReference[] _images = [
        new UrlReference { Id = Guid.NewGuid(), Url = "https://localhost:7236/uploads/sample/image1.png" },
        new UrlReference { Id = Guid.NewGuid(), Url = "https://localhost:7236/uploads/sample/image2.png" },
        new UrlReference { Id = Guid.NewGuid(), Url = "https://localhost:7236/uploads/sample/image3.png" }
    ];
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    //public ProjectStepDataDetails[] Steps { get; set; } = Array.Empty<ProjectStepDataDetails>();
    //// public MaterialDetails[] Materials { get; set; } = Array.Empty<MaterialDetails>();
    public UrlReference[] References => _images;
    //public UrlReference[] Pictures { get; set; } = Array.Empty<UrlReference>();
    public ColorGroupDetails[] Groups { get; set; } = Array.Empty<ColorGroupDetails>();
    public ProjectCostDetails CostBreakdown { get; set; }  
}

public class ProjectCostDetails
{
    public required Guid Id { get; init; }
    public required ElectricityCostDetails Electricity { get; init; }
    public Dictionary<string, LaborCostDetails> Labor { get; init; }
    public Dictionary<string, IReadOnlyCollection<MaterialsCostDetails>> Materials { get; init; }
}

public class MaterialsCostDetails
{
    public required Guid MaterialId { get; init; }
    public required string Description { get; init; }
    public required double Quantity { get; init; }
    public required string Category { get; init; }
    public required MoneyDetails TotalCost { get; init; } 
}

public class LaborCostDetails
{
    public required double SpentHours { get; init; }
    public required MoneyDetails TotalCost { get; init; } 
}

public class ElectricityCostDetails
{
    public required MoneyDetails CostPerKWh { get; init; }
    public required double PrintingTimeInHours { get; init; }
    public required MoneyDetails TotalCost { get; init; } 
} 

public record MoneyDetails
{
    public double Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Amount:N1} {Currency}";
    }
}

public record QuantityDetails
{
    public double Amount { get; set; }
    public EnumReference Unit { get; set; } = new(0, string.Empty);
}

public enum MaterialUnit
{
    Gram = 10,
    Mililiter = 20,
    Meter = 30,
    Each = 40
}

public class UrlReference
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
} 

public class ColorGroupDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ColorSectionDetails[] Sections { get; set; } = [];
}

public class ColorSectionDetails
{
    public Guid Id { get; set; }
    public string ReferenceColor { get; set; } = string.Empty;
    public ColorZone Zone { get; set; }
}