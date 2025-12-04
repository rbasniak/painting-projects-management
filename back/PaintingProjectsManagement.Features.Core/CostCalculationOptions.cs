using System.ComponentModel;
using rbkApiModules.Commons.Core.Features.ApplicationOptions;

namespace PaintingProjectsManagement.Features.Core;

public class CostCalculationOptions : IApplicationOptions
{
    [DisplayName("Printer Consumption in Kwh")]
    public double PrinterConsumptioninKwh { get; set; } = 0.18;

    [DisplayName("Resin Waste Coefficient (%)")]
    public double ResinWasteCoefficient { get; set; } = 0.55;

    [DisplayName("Average Kilowatt Per Hour Price ($)")]
    public double AverageKilowattPerHourPrice { get; set; } = 0;

    [DisplayName("Materials Markup Coefficient (%)")]
    public double MarkupPercentage { get; set; } = 0.00;

    [DisplayName("Mililiters Per Drop of Paint")]
    public double MililitersPerPaintDrop { get; set; } = 0.09;

    [DisplayName("Mililiters Per Spray Press")]
    public double MililiterPerSpray { get; set; } = 0.05;
}
