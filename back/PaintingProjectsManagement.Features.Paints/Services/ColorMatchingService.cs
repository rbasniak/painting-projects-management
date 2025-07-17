namespace PaintingProjectsManagement.Features.Paints;

public class ColorHelper
{
    private static readonly double MAX_DISTANCE;

    static ColorHelper()
    {
        MAX_DISTANCE = Math.Sqrt(255 * 255 * 3);
    }

    public static double CalculateColorDistance(string hexColor1, string hexColor2)
    {
        // Convert hex colors to RGB
        (int r1, int g1, int b1) = HexToRgb(hexColor1);
        (int r2, int g2, int b2) = HexToRgb(hexColor2);

        // Calculate the Euclidean distance
        var distance = Math.Sqrt(Math.Pow(r2 - r1, 2) + Math.Pow(g2 - g1, 2) + Math.Pow(b2 - b1, 2));
        return distance;
    }

    static (int, int, int) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return (r, g, b);
        }
        else
        {
            throw new ArgumentException("Invalid hex color format.");
        }
    }

    public static double CalculateColorSimilarity(string hexColor1, string hexColor2)
    {
        var distance = CalculateColorDistance(hexColor1, hexColor2);

        return 1.0 - distance / MAX_DISTANCE;
    }
}