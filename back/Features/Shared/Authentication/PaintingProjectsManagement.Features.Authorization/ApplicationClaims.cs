namespace PaintingProjectsManagement.Features;

public static class Claims
{
    public const string MANAGE_PAINTS = "MANAGE_PAINTS";
}

public static class ApplicationClaims
{
    public static ClaimValue MANAGE_PAINTS => new ClaimValue(Claims.MANAGE_PAINTS, "Paints Management");
}

public class ClaimValue
{
    public ClaimValue(string name, string description)
    {
        Name = name;
        Description = description;
    }
    public string Name { get; set; }
    public string Description { get; set; }
}