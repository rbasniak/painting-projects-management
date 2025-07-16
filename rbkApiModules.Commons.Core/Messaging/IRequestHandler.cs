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

public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, CommandResponse<TResult>> where TResult : class
    where TCommand : ICommand<TResult>
{ }

public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, QueryResponse<TResult>> where TResult : class
    where TQuery : IQuery<TResult>
{ } 