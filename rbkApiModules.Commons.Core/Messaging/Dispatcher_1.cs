using Microsoft.Extensions.DependencyInjection;

namespace rbkApiModules.Commons.Core;

public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"Handler not found for {request.GetType()}");

        var behaviors = _serviceProvider.GetServices(typeof(IPipelineBehavior<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse)))
            .Cast<object>().ToList();

        Func<Task<TResponse>> handle = () =>
            (Task<TResponse>)handlerType.GetMethod("Handle").Invoke(handler, new object[] { request, cancellationToken });

        // Pipeline behavior execution (in reverse order like MediatR)
        foreach (var behavior in behaviors.Reverse<object>())
        {
            var next = handle;
            handle = () =>
            {
                var method = behavior.GetType().GetMethod("Handle");
                return (Task<TResponse>)method.Invoke(behavior, new object[] { request, cancellationToken, next });
            };
        }

        return await handle();
    }

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(typeof(TNotification));
        var handlers = _serviceProvider.GetServices(handlerType).Cast<object>().ToList();

        foreach (var handler in handlers)
        {
            await (Task)handlerType.GetMethod("Handle").Invoke(handler, new object[] { notification, cancellationToken });
        }
    }
}
