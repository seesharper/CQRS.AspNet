using System.Reflection;
using CQRS.AspNet.MetaData;
using CQRS.Command.Abstractions;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CQRS.AspNet;

/// <summary>
/// Extends the <see cref="IEndpointRouteBuilder"/> with methods to map commands and queries to routes.
/// </summary>
public static class RouteBuilderExtensions
{
    private static readonly MethodInfo CreateTypedQueryDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedQueryDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CreateTypedQueryDelegateForPostMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedQueryDelegateForPost), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CreateTypedCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedCommandDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CreateTypedDeleteCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedDeleteCommandDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CreateTypedCommandDelegateWithResultMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedCommandDelegateWithResult), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CreateTypedDeleteCommandDelegateWithResultMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedDeleteCommandDelegateWithResult), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo MapGetMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapGet), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapPostMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPost), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapDeleteMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapDelete), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapPatchMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPatch), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapPutMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPut), BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>
    /// Maps command and queries that are decorated with <see cref="GetAttribute"/>, <see cref="PostAttribute"/>, <see cref="DeleteAttribute"/>, <see cref="PatchAttribute"/>, <see cref="PutAttribute"/> attributes.
    /// </summary>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="assembly">The <see cref="Assembly"/> for which to scan for commands and queries</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining calls. </returns>
    public static IEndpointRouteBuilder MapCqrsEndpoints(this IEndpointRouteBuilder builder, Assembly? assembly = null)
    {
        var allTypes = assembly?.GetTypes() ?? Assembly.GetCallingAssembly()!.GetTypes();
        var typeWithRouteAttribute = allTypes.Where(t => t.GetCustomAttributes<RouteBaseAttribute>().Any());
        foreach (var type in typeWithRouteAttribute)
        {
            var routeAttributes = type.GetCustomAttributes<RouteBaseAttribute>();
            foreach (var routeAttribute in routeAttributes)
            {
                var method = routeAttribute switch
                {
                    GetAttribute => MapGetMethod,
                    PostAttribute => MapPostMethod,
                    DeleteAttribute => MapDeleteMethod,
                    PatchAttribute => MapPatchMethod,
                    PutAttribute => MapPutMethod,
                    _ => null
                };

                method?.MakeGenericMethod(type).Invoke(null, [builder, routeAttribute.Route]);
            }
        }
        return builder;
    }

    /// <summary>
    /// Maps the given <typeparamref name="TQuery"/> to the specified GET route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to be mapped to a GET endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapGet<TQuery>(this IEndpointRouteBuilder builder, string pattern)
    {
        //TODO throw exception if this is not a query
        if (!typeof(TQuery).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
        {
            throw new InvalidOperationException($"Type {typeof(TQuery).Name} is not a query. Only queries can be used in get endpoints");
        }
        var queryInterface = typeof(TQuery).GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IQuery<>));
        var resultType = queryInterface.GetGenericArguments()[0];

        var createTypedDelegateMethod = CreateTypedQueryDelegateMethod.MakeGenericMethod(typeof(TQuery), resultType);
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapGet(pattern, typedDelegate);
    }

    /// <summary>
    /// Maps the given <typeparamref name="TCommandOrQuery"/> to the specified POST route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TCommandOrQuery">The type of command or query to be mapped to a POST endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapPost<TCommandOrQuery>(this IEndpointRouteBuilder builder, string pattern)
    {
        if (typeof(TCommandOrQuery).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
        {
            var queryInterface = typeof(TCommandOrQuery).GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IQuery<>));
            var resultType = queryInterface.GetGenericArguments()[0];

            var createTypedDelegateMethod = CreateTypedQueryDelegateForPostMethod.MakeGenericMethod(typeof(TCommandOrQuery), resultType);
            var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;
            return builder.MapPost(pattern, typedDelegate);
        }

        return builder.MapPost(pattern, (Delegate)GetCreateTypedDelegateMethod<TCommandOrQuery>().Invoke(null, null)!);
    }

    private static MethodInfo GetCreateTypedDelegateMethod<TCommand>()
    {
        var commandType = GetCommandType(typeof(TCommand));
        if (commandType != null)
        {
            var resultType = commandType.GetGenericArguments()[0];
            return CreateTypedCommandDelegateWithResultMethod.MakeGenericMethod(typeof(TCommand), resultType);
        }
        else
        {
            return CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        }
    }

    private static MethodInfo GetCreateTypedDeleteDelegateMethod<TCommand>()
    {
        var commandType = GetCommandType(typeof(TCommand));

        if (commandType != null)
        {
            var resultType = commandType.GetGenericArguments()[0];
            return CreateTypedDeleteCommandDelegateWithResultMethod.MakeGenericMethod(typeof(TCommand), resultType);
        }
        else
        {
            return CreateTypedDeleteCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        }
    }

    private static Type? GetCommandType(Type type)
    {
        while (type.BaseType != null && type.BaseType != typeof(object))
        {
            if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(Command<>))
            {
                return type.BaseType;
            }
            type = type.BaseType;
        }
        return null;
    }


    /// <summary>
    /// Maps the given <typeparamref name="TCommand"/> to the specified PATCH route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be mapped to a PATCH endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapPatch<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        return builder.MapPatch(pattern, (Delegate)GetCreateTypedDelegateMethod<TCommand>().Invoke(null, null)!);
    }

    /// <summary>
    /// Maps the given <typeparamref name="TCommand"/> to the specified PUT route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be mapped to a PUT endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapPut<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        return builder.MapPut(pattern, (Delegate)GetCreateTypedDelegateMethod<TCommand>().Invoke(null, null)!);
    }

    /// <summary>
    /// Maps the given <typeparamref name="TCommand"/> to the specified DELETE route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be mapped to a DELETE endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapDelete<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        return builder.MapDelete(pattern, (Delegate)GetCreateTypedDeleteDelegateMethod<TCommand>().Invoke(null, null)!);
    }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task> CreateTypedCommandDelegate<TCommand>()
    {
        return async (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand command) =>
        {
            MapRouteValues(request, command);
            await commandExecutor.ExecuteAsync(command, CancellationToken.None);
        };
    }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task<TResult>> CreateTypedCommandDelegateWithResult<TCommand, TResult>() where TCommand : Command<TResult>
    {
        return async (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand command) =>
        {
            MapRouteValues(request, command);
            await commandExecutor.ExecuteAsync(command, CancellationToken.None);
            return command.GetResult()!;
        };
    }

    private static void MapRouteValues<TCommand>(HttpRequest request, TCommand command)
    {
        var routeValues = request.RouteValues;
        if (routeValues.Count > 0)
        {
            foreach (var routeValue in routeValues)
            {
                var property = typeof(TCommand).GetProperty(routeValue.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    property.SetValue(command, Convert.ChangeType(routeValue.Value, property.PropertyType));
                }
            }
        }
    }

    private static Func<ICommandExecutor, TCommand, Task> CreateTypedDeleteCommandDelegate<TCommand>()
    {
        return async (ICommandExecutor commandExecutor, [AsParameters] TCommand command) =>
        {
            await commandExecutor.ExecuteAsync(command, CancellationToken.None);
        };
    }

    private static Func<ICommandExecutor, TCommand, Task<TResult>> CreateTypedDeleteCommandDelegateWithResult<TCommand, TResult>() where TCommand : Command<TResult>
    {
        return async (ICommandExecutor commandExecutor, [AsParameters] TCommand command) =>
        {
            await commandExecutor.ExecuteAsync(command, CancellationToken.None);
            return command.GetResult()!;
        };
    }

    private static Func<IQueryExecutor, TQuery, Task<TResult>> CreateTypedQueryDelegate<TQuery, TResult>() where TQuery : IQuery<TResult>
    {
        return async (IQueryExecutor queryExecutor, [AsParameters] TQuery query) => await queryExecutor.ExecuteAsync(query, CancellationToken.None);
    }

    private static Func<IQueryExecutor, TQuery, Task<TResult>> CreateTypedQueryDelegateForPost<TQuery, TResult>() where TQuery : IQuery<TResult>
    {
        return async (IQueryExecutor queryExecutor, [FromBody] TQuery query) => await queryExecutor.ExecuteAsync(query, CancellationToken.None);
    }
}
