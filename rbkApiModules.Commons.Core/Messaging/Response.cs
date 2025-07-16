using Microsoft.AspNetCore.Mvc;

namespace rbkApiModules.Commons.Core;

public abstract class BaseResponse 
{
    public bool IsValid { get; protected set; }

    public ProblemDetails? Error { get; protected set; }

    public object? Data { get; protected set; }
} 

public sealed class CommandResponse : BaseResponse
{
    internal CommandResponse() 
    {
    }

    public static CommandResponse Success()
    {
        return new CommandResponse()
        {
            IsValid = true
        };
    }

    public static CommandResponse Success(object result)
    {
        return new CommandResponse()
        {
            Error = null,
            IsValid = true,
            Data = result
        };
    }

    public static CommandResponse Failure(ProblemDetails problem)
    {
        return new CommandResponse()
        {
            Error = problem,
            IsValid = false,
            Data = null
        };
    } 
}

public sealed class QueryResponse : BaseResponse
{
    internal QueryResponse()
    {
    }

    public static QueryResponse Success()
    {
        return new QueryResponse
        {
            IsValid = true
        };
    }

    public static QueryResponse Success(object result)
    {
        return new QueryResponse()
        {
            Error = null,
            IsValid = true,
            Data = result
        };
    }

    public static QueryResponse Failure(ProblemDetails problem)
    {
        return new QueryResponse()
        {
            Error = problem,
            IsValid = false,
            Data = null
        };
    }
}
