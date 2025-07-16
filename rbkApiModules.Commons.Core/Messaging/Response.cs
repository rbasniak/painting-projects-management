using Microsoft.AspNetCore.Mvc;

namespace rbkApiModules.Commons.Core;

public interface ITypedResponse<T> where T : class
{
    bool IsValid { get; }
    ProblemDetails? Error { get; }
    T Data { get; }
}

public interface IUntypedResponse
{
    bool IsValid { get; }
    ProblemDetails? Error { get; }
}

public abstract class BaseResponse : IUntypedResponse
{
    public bool IsValid { get; }
    public ProblemDetails? Error { get; }

    protected BaseResponse()
    {
        IsValid = true;
        Error = null;
    }

    protected BaseResponse(ProblemDetails error)
    {
        IsValid = false;
        Error = error;
    }
}

public abstract class BaseResponse<T> : BaseResponse, ITypedResponse<T> where T : class
{
    public T? Data { get; }

    protected BaseResponse(T data): base()
    {
        Data = data;
    }

    protected BaseResponse(ProblemDetails error): base(error)
    {
    }
}

public sealed class CommandResponse : BaseResponse
{

    internal CommandResponse(ProblemDetails error) : base(error)
    {
    }

    internal CommandResponse(): base()
    {
    }

    public static CommandResponse Success()
    {
        return new CommandResponse();
    }

    public static CommandResponse<T> Success<T>(T result) where T : class 
    {
        return new CommandResponse<T>(result);
    }

    public static CommandResponse Failure(ProblemDetails problem)
    {
        return new CommandResponse(problem);
    }

    public static CommandResponse<T> Failure<T>(ProblemDetails problem) where T : class 
    {
        return new CommandResponse<T>(problem);
    }
}

public sealed class CommandResponse<T> : BaseResponse<T> where T : class 
{
    internal CommandResponse(ProblemDetails error) : base(error)
    {
    }

    internal CommandResponse(T data) : base(data)
    {
    }
}

public sealed class QueryResponse : BaseResponse
{
    internal QueryResponse(ProblemDetails error) : base(error)
    {
    }

    internal QueryResponse() : base()
    {
    }

    public static QueryResponse<T> Success<T>(T result) where T : class 
    {
        return new QueryResponse<T>(result);
    }

    public static QueryResponse<T> Failure<T>(ProblemDetails problem) where T : class 
    {
        return new QueryResponse<T>(problem);
    }

    public static QueryResponse Failure(ProblemDetails problem)  
    {
        return new QueryResponse(problem);
    }
}

public sealed class QueryResponse<T> : BaseResponse<T> where T : class 
{
    internal QueryResponse(ProblemDetails error) : base(error)
    {
    }

    internal QueryResponse(T data) : base(data)
    {
    }
}