using System.Reflection;

namespace OnlyBurger.Api.Cqrs;

/// <summary>
/// Discovers and registers every <see cref="ICommandHandler{TCommand,TResult}"/> and
/// <see cref="IQueryHandler{TQuery,TResult}"/> in the application assembly so new handlers
/// are wired up automatically without touching <c>Program.cs</c>.
/// </summary>
public static class CqrsRegistration
{
    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        var assembly = Assembly.GetExecutingAssembly();
        var openHandlerInterfaces = new[]
        {
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>)
        };

        var implementations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var implementation in implementations)
        {
            var handlerInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && openHandlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, implementation);
            }
        }

        return services;
    }
}
