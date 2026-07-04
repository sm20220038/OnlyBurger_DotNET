namespace OnlyBurger.Api.Cqrs;

/// <summary>
/// Marker for a write operation that changes system state and returns <typeparamref name="TResult"/>.
/// Part of the CQRS command side.
/// </summary>
public interface ICommand<TResult>
{
}

/// <summary>
/// Marker for a read operation that returns <typeparamref name="TResult"/> without changing state.
/// Part of the CQRS query side.
/// </summary>
public interface IQuery<TResult>
{
}

/// <summary>
/// Handles a single <typeparamref name="TCommand"/>. Each command has exactly one handler.
/// </summary>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles a single <typeparamref name="TQuery"/>. Each query has exactly one handler.
/// </summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
