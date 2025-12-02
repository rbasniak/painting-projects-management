using System.ComponentModel;
using rbkApiModules.Commons.Core.Features.ApplicationOptions;

namespace PaintingProjectsManagement.Features.Core;

public class CostCalculationOptions : IApplicationOptions
{
    [DisplayName("Print Height Per Minute (mm/h)")]
    public double PrintHeightPerMinute { get; set; } = 20;
    
    [DisplayName("Resin Waste Coefficient (%)")]
    public double ResinWasteCoefficient { get; set; } = 1.5;

    [DisplayName("Default Wall Thickness (mm)")]
    public double DefaultWallThickness { get; set; } = 1.5;

    [DisplayName("Average Kilowatt Per Hour Price ($)")]
    public double AverageKilowattPerHourPrice { get; set; } = 0;

    [DisplayName("Markup Coefficient (%)")]
    public double MarkupPercentage { get; set; } = 1.15;

    [DisplayName("Mililiters per drop of paint")]
    public double MililitersPerPaintDrop { get; set; } = 0.09;
}
