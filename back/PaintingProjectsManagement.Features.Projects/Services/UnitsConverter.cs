namespace PaintingProjectsManagement.Features.Projects;

internal interface IUnitsConverter
{
    public Quantity Convert(Quantity quantity, MaterialUnit targetUnit);
}

internal class UnitsConverter : IUnitsConverter
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
        throw new InvalidOperationException(
            $"Cannot convert from {quantity.Unit} to {targetUnit}. These units are incompatible.");
    }

    private static bool IsLengthUnit(MaterialUnit unit) =>
        unit is MaterialUnit.Centimeter or MaterialUnit.Meter;

    private static bool IsWeightUnit(MaterialUnit unit) =>
        unit is MaterialUnit.Gram or MaterialUnit.Kilogram;

    private static bool IsVolumeUnit(MaterialUnit unit) =>
        unit is MaterialUnit.Milliliter or MaterialUnit.Liter;

    private static Quantity ConvertLength(Quantity quantity, MaterialUnit targetUnit)
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

    private static Quantity ConvertWeight(Quantity quantity, MaterialUnit targetUnit)
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

    private static Quantity ConvertVolume(Quantity quantity, MaterialUnit targetUnit)
    {
        // Convert to base unit (milliliters)
        double milliliters = quantity.Unit switch
        {
            MaterialUnit.Milliliter => quantity.Value,
            MaterialUnit.Liter => quantity.Value * 1000,
            _ => throw new InvalidOperationException($"Unsupported volume unit: {quantity.Unit}")
        };

        // Convert from base unit to target
        double result = targetUnit switch
        {
            MaterialUnit.Milliliter => milliliters,
            MaterialUnit.Liter => milliliters / 1000,
            _ => throw new InvalidOperationException($"Unsupported volume unit: {targetUnit}")
        };

        return new Quantity(result, targetUnit);
    }
}
