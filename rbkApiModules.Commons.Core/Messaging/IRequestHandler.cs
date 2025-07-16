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

public interface IQueryHandler<TQuery> : IRequestHandler<TQuery, QueryResponse> 
    where TQuery : IQuery
{ }

// New typed query handler interface for module-to-module communication
public interface ITypedQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, QueryResponse<TResponse>>
    where TQuery : IQuery<TResponse>
{ } 