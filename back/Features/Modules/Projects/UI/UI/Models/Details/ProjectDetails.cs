using PaintingProjectsManagement.Features.Projects.UI;
using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public record ProjectHeader
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
    public bool IsArchived { get; set; }
}

public class ProjectDetails
{ 
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsArchived { get; set; }

    //public ProjectStepDataDetails[] Steps { get; set; } = Array.Empty<ProjectStepDataDetails>();
    //// public MaterialDetails[] Materials { get; set; } = Array.Empty<MaterialDetails>();
    public UrlReference[] References { get; set; } = [];
    public UrlReference[] Pictures { get; set; } = [];
    public ColorGroupDetails[] Groups { get; set; } = Array.Empty<ColorGroupDetails>();
    public ProjectCostDetails CostBreakdown { get; set; } = ProjectCostDetails.Empty;
}

public class ProjectCostDetails
{
    public required Guid Id { get; init; }
    public required ElectricityCostDetails Electricity { get; init; }
    public Dictionary<string, LaborCostDetails> Labor { get; init; } = new();
    public Dictionary<string, IReadOnlyCollection<MaterialsCostDetails>> Materials { get; init; } = new();

    public static ProjectCostDetails Empty => new()
    {
        Id = Guid.Empty,
        Electricity = new ElectricityCostDetails
        {
            CostPerKWh = MoneyDetails.Empty,
            PrintingTimeInHours = 0,
            TotalCost = MoneyDetails.Empty
        },
        Labor = new Dictionary<string, LaborCostDetails>(),
        Materials = new Dictionary<string, IReadOnlyCollection<MaterialsCostDetails>>()
    };
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

    public static MoneyDetails Empty => new()
    {
        Amount = 0,
        Currency = "DKK"
    };

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
    public Guid? PickedColorId { get; set; }
    public ColorZone Zone { get; set; }

    public ColorMatchDetails[] SuggestedColors { get; set; } = [];
}

public class ColorMatchDetails
{
    public Guid PaintColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public double Distance { get; set; }
}