namespace rbkApiModules.Commons.Core;

public class InternalException : Exception
{
    public InternalException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
    public InternalException(string message, Exception innerException, int statusCode = 500) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
    public int StatusCode { get; }
}
