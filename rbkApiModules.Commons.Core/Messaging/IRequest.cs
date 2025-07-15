namespace rbkApiModules.Commons.Core;

public interface IRequest<TResponse> { }

public interface ICommand : IRequest<CommandResponse> { }
public interface ICommand<TResult> : IRequest<CommandResponse<TResult>> { }

public interface IQuery<TResult> : IRequest<QueryResponse<TResult>> { }

public interface INotification { }