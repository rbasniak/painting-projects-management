namespace rbkApiModules.Commons.Core;

public interface IRequest<TResponse> { }

public interface ICommand : IRequest<CommandResponse> { }
public interface IQuery : IRequest<QueryResponse> { }

public interface INotification { }