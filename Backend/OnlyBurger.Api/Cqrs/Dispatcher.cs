namespace OnlyBurger.Api.Cqrs;

/// <summary>
/// Central entry point that routes a command or query to its registered handler.
/// Controllers depend on this rather than on concrete handlers, keeping the read and
/// write sides decoupled from the API surface.
/// </summary>
public interface IDispatcher
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, cancellationToken);
    }
}
