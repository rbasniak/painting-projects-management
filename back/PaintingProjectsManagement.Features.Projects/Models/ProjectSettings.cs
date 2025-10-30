using rbkApiModules.Commons.Core.Features.ApplicationOptions;
using System.ComponentModel;

namespace PaintingProjectsManagement.Features.Projects;

public class ProjectSettings : IApplicationOptions
{
    [DefaultValue(0.18)]
    public double PrinterConsumptioninKwh { get; set; }

    [DefaultValue(0)]
    public Money ElectricityCostPerKwh { get; set; }

    [DefaultValue(0)]
    public Money LaborCostPerHour { get; set; }
}
