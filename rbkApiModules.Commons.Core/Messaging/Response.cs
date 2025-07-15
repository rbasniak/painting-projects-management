namespace rbkApiModules.Commons.Core;

public class CommandResponse
{
    public bool IsValid { get; }
    public string Error { get; }

    public CommandResponse(bool isValid = true, string error = null)
    {
        IsValid = isValid;
        Error = error;
    }

    public static CommandResponse Success()
    {
        return new CommandResponse(true);
    }

    public static CommandResponse<T> Success<T>(T result)
    {
        return new CommandResponse<T>(result);
    }
}

public class CommandResponse<T> : CommandResponse
{
    public T Result { get; }

    public CommandResponse(T result, bool isValid = true, string error = null)
        : base(isValid, error)
    {
        Result = result;
    }
}

public class QueryResponse
{
    public bool IsValid { get; }
    public string Error { get; }

    public QueryResponse(bool isSuccess = true, string error = null)
    {
        IsValid = isSuccess;
        Error = error;
    }

    public static QueryResponse<T> Success<T>(T result)
    {
        return new QueryResponse<T>(result);
    }
}

public class QueryResponse<T> : QueryResponse
{
    public T Result { get; }

    public QueryResponse(T result, bool success = true, string error = null)
        : base(success, error)
    {
        Result = result;
    }
}