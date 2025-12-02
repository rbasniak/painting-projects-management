namespace PaintingProjectsManagement.Features.Projects;

internal interface IUnitsConverter
{
    public Quantity Convert(Quantity quantity, MaterialUnit targetUnit);
}

internal class UnitsConverter (ProjectSettings projectSettings): IUnitsConverter
{
    public Quantity Convert(Quantity quantity, MaterialUnit targetUnit)
    {
        if (quantity.Unit == targetUnit)
        {
            return quantity;
        }

        // Length conversions
        if (IsLengthUnit(quantity.Unit) && IsLengthUnit(targetUnit))
        {
            return ConvertLength(quantity, targetUnit);
        }

        // Weight conversions
        if (IsWeightUnit(quantity.Unit) && IsWeightUnit(targetUnit))
        {
            return ConvertWeight(quantity, targetUnit);
        }

        // Volume conversions
        if (IsVolumeUnit(quantity.Unit) && IsVolumeUnit(targetUnit))
        {
            return ConvertVolume(quantity, targetUnit);
        }

        // Count units (Drop, Unit) cannot be converted to each other or to other types
        throw new InvalidOperationException($"Cannot convert from {quantity.Unit} to {targetUnit}. These units are incompatible.");
    }

    private bool IsLengthUnit(MaterialUnit unit) =>
        unit is MaterialUnit.Centimeter or MaterialUnit.Meter;

    private bool IsWeightUnit(MaterialUnit unit) =>
        unit is MaterialUnit.Gram or MaterialUnit.Kilogram;

    private bool IsVolumeUnit(MaterialUnit unit) =>
        unit is MaterialUnit.Mililiter or MaterialUnit.Liter or MaterialUnit.Drop or MaterialUnit.Spray;

    private Quantity ConvertLength(Quantity quantity, MaterialUnit targetUnit)
    {
        // Convert to base unit (centimeters)
        double centimeters = quantity.Unit switch
        {
            MaterialUnit.Centimeter => quantity.Value,
            MaterialUnit.Meter => quantity.Value * 100,
            _ => throw new InvalidOperationException($"Unsupported length unit: {quantity.Unit}")
        };

        // Convert from base unit to target
        double result = targetUnit switch
        {
            MaterialUnit.Centimeter => centimeters,
            MaterialUnit.Meter => centimeters / 100,
            _ => throw new InvalidOperationException($"Unsupported length unit: {targetUnit}")
        };

        return new Quantity(result, targetUnit);
    }

    private Quantity ConvertWeight(Quantity quantity, MaterialUnit targetUnit)
    {
        // Convert to base unit (grams)
        double grams = quantity.Unit switch
        {
            MaterialUnit.Gram => quantity.Value,
            MaterialUnit.Kilogram => quantity.Value * 1000,
            _ => throw new InvalidOperationException($"Unsupported weight unit: {quantity.Unit}")
        };

        // Convert from base unit to target
        double result = targetUnit switch
        {
            MaterialUnit.Gram => grams,
            MaterialUnit.Kilogram => grams / 1000,
            _ => throw new InvalidOperationException($"Unsupported weight unit: {targetUnit}")
        };

        return new Quantity(result, targetUnit);
    }

    private Quantity ConvertVolume(Quantity quantity, MaterialUnit targetUnit)
    {
        // Convert to base unit (mililiters)
        double mililiters = quantity.Unit switch
        {
            MaterialUnit.Mililiter => quantity.Value,
            MaterialUnit.Liter => quantity.Value * 1000,
            MaterialUnit.Drop => quantity.Value * projectSettings.MililiterPerDrop,
            MaterialUnit.Spray => quantity.Value * projectSettings.MililiterPerSpray,
            _ => throw new InvalidOperationException($"Unsupported volume unit: {quantity.Unit}")
        };

        // Convert from base unit to target
        double result = targetUnit switch
        {
            MaterialUnit.Mililiter => mililiters,
            MaterialUnit.Liter => mililiters / 1000,
            _ => throw new InvalidOperationException($"Unsupported volume unit: {targetUnit}")
        };

        return new Quantity(result, targetUnit);
    }
}
