namespace PaintingProjectsManagement.UI.Modules.Shared;

public static class EnumReferenceConstants
{
    public static class PackageContentUnit
    {
        public const string Ml = "ml";
        public const string G = "g";
        public const string Units = "units";
        public const string Cm = "cm";
        public const string M = "m";
        public const string L = "l";
        public const string Kg = "kg";
        public const string Pieces = "pieces";
        public const string Sets = "sets";
        public const string Bottles = "bottles";
        public const string Tubes = "tubes";
        public const string Jars = "jars";
        public const string Cans = "cans";
        public const string Boxes = "boxes";
        public const string Bags = "bags";
        public const string Rolls = "rolls";
        public const string Sheets = "sheets";
        public const string Strips = "strips";
        public const string Pads = "pads";
        public const string Brushes = "brushes";
        public const string Sponges = "sponges";
        public const string Sticks = "sticks";
        public const string Pencils = "pencils";
        public const string Markers = "markers";
        public const string Pens = "pens";
        public const string Erasers = "erasers";
        public const string Sharpeners = "sharpeners";
        public const string Rulers = "rulers";
        public const string Compasses = "compasses";
    }
}

public record EnumReference(int Id, string Value); 