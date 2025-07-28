using Microsoft.AspNetCore.Mvc;

namespace rbkApiModules.Commons.Core;

public abstract class BaseResponse
{
    private ProblemDetails _error = default!;
    private object _data = default!;

    public bool IsValid { get; protected set; }

    public ProblemDetails Error
    {
        get
        {
            if (IsValid)
            {
                throw new InvalidOperationException("Cannot access Error when response is valid.");
            }

            return _error;
        }
        protected set
        {
            _error = value;
        }
    }

    public object Data
    {
        get
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Cannot access Data when response is not valid.");
            }

            return _data;
        }
        protected set
        {
            _data = value;
        }
    }
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

// New typed query response for module-to-module communication
public sealed class QueryResponse<T> : BaseResponse
{
    private T _data = default!;

    internal QueryResponse()
    {
    }

    public static QueryResponse<T> Success(T result)
    {
        return new QueryResponse<T>()
        {
            Error = null,
            IsValid = true,
            Data = result
        };
    }

    public static QueryResponse<T> Failure(ProblemDetails problem)
    {
        return new QueryResponse<T>()
        {
            Error = problem,
            IsValid = false,
            Data = default!
        };
    }

    public new T Data
    {
        get
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Cannot access Data when response is not valid.");
            }

            return _data;
        }
        private set
        {
            _data = value;
        }
    }
}
