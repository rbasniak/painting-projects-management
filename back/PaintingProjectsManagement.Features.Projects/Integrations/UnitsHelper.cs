namespace PaintingProjectsManagement.Features.Projects;

internal static class UnitsHelper
{
    public static MaterialUnit Convert(string unit)
    {
        switch (unit.ToLowerInvariant())
        {
            case "unit":
            case "units":
                return MaterialUnit.Unit;
            case "centimeter":
            case "centimeters":
                return MaterialUnit.Centimeter;
            case "meter":
            case "meters":
                return MaterialUnit.Meter;
            case "gram":
            case "grams":
                return MaterialUnit.Gram;
            case "kilogram":
            case "kilograms":
                return MaterialUnit.Kilogram;
            case "mililiter":
            case "mililiters":
                return MaterialUnit.Milliliter;
            case "liter":
            case "liters":
                return MaterialUnit.Liter;
            case "each":
                return MaterialUnit.Unit;
            default:
                throw new ArgumentException($"Unsupported unit: {unit}", nameof(unit));
        }
    }
}
