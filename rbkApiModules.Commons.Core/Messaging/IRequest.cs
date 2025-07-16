namespace rbkApiModules.Commons.Core;

public interface IRequest<TResponse> { }

public interface ICommand : IRequest<CommandResponse> { }
public interface IQuery : IRequest<QueryResponse> { }

// New typed query interface for module-to-module communication
public interface IQuery<TResponse> : IRequest<QueryResponse<TResponse>> { }

public interface INotification { }