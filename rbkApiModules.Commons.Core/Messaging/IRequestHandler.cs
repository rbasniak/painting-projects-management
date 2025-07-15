using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace rbkApiModules.Commons.Core;

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, CommandResponse>
    where TCommand : ICommand
{ }

public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, CommandResponse<TResult>>
    where TCommand : ICommand<TResult>
{ }

// Query handlers
public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, QueryResponse<TResult>>
    where TQuery : IQuery<TResult>
{ } 