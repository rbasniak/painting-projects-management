namespace PaintingProjectsManagement.Infrastructure;

public class InternalValidationException : Exception
{
    public InternalValidationException(Dictionary<string, string[]> summary) : base(string.Empty)
    {
        Summary = summary;
    }

    public Dictionary<string, string[]> Summary { get; }
}
