using PaintingProjectsManagement.UI.Modules.Projects;

namespace PaintingProjectsManagement.Features.Projects.UI;

public class ProjectCostBreakdown  
{
    public Guid ProjectId { get; set; }
    public ElectricityCost Electricity { get; set; }
    public LaborCost Labor { get; set; }
    public Dictionary<string, IReadOnlyCollection<MaterialsCost>> Materials { get; set; }
}

public class ElectricityCost
{
    public MoneyDetails CostPerKWh { get; set; }
    public double PrinterConsumptionInKWh { get; set; }
    public double TotalPrintingTimeInHours { get; set; }
    public MoneyDetails TotalCost { get; set; }
}

public class LaborCost
{
    public double SpentHours { get; set; }
    public MoneyDetails CostPerHour { get; set; }
    public MoneyDetails TotalCost { get; set; } 
}

public class MaterialsCost
{
    public Guid MaterialId { get; set; }
    public string Description { get; set; } = string.Empty;
    public QuantityDetails Quantity { get; set; }
    public MoneyDetails CostPerUnit { get; set; }
    public MoneyDetails TotalCost { get; set; }
}
