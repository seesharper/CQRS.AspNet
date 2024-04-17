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
    private static readonly MethodInfo CreateTypedCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedCommandDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CreateTypedDeleteCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedDeleteCommandDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CreateTypedCommandDelegateWithResultMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedCommandDelegateWithResult), BindingFlags.NonPublic | BindingFlags.Static)!;
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
        var openGenericExecuteAsyncMethod = typeof(IQueryExecutor).GetMethod(nameof(IQueryExecutor.ExecuteAsync))!;
        var queryInterface = typeof(TQuery).GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
        var resultType = queryInterface.GetGenericArguments()[0];

        var createTypedDelegateMethod = CreateTypedQueryDelegateMethod.MakeGenericMethod(typeof(TQuery), resultType);
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapGet(pattern, typedDelegate);
    }

    /// <summary>
    /// Maps the given <typeparamref name="TCommand"/> to the specified POST route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to be mapped to a POST endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapPost<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        // check if TCommand inherits from Command<IResult>
        Type? baseType = typeof(TCommand).BaseType;
        if (baseType != null && baseType.IsGenericType == true && baseType.GetGenericTypeDefinition() == typeof(Command<>))
        {
            var resultType = baseType.GetGenericArguments()[0];
            var createTypedDelegateMethod = CreateTypedCommandDelegateWithResultMethod.MakeGenericMethod(typeof(TCommand), resultType);
            var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

            return builder.MapPost(pattern, typedDelegate);
        }
        else
        {
            var createTypedDelegateMethod = CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
            var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;
            return builder.MapPost(pattern, typedDelegate);
        }
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
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedDeleteCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapDelete(pattern, typedDelegate);
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
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapPatch(pattern, typedDelegate);
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
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapPut(pattern, typedDelegate);
    }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task> CreateTypedCommandDelegate<TCommand>()
    {
        return (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand command) =>
        {
            MapRouteValues(request, command);
            return commandExecutor.ExecuteAsync(command, CancellationToken.None);
        };
    }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task<TResult>> CreateTypedCommandDelegateWithResult<TCommand, TResult>() where TCommand : Command<TResult> where TResult : IResult
    {
        return (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand command) =>
        {
            MapRouteValues(request, command);
            commandExecutor.ExecuteAsync(command, CancellationToken.None);
            return Task.FromResult(command.GetResult()!);
        };
    }

    private static void MapRouteValues<TCommand>(HttpRequest request, TCommand command)
    {
        var routeValues = request.RouteValues;
        if (routeValues.Count > 0)
        {
            foreach (var routeValue in routeValues)
            {
                var property = typeof(TCommand).GetProperty(routeValue.Key, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    property.SetValue(command, Convert.ChangeType(routeValue.Value, property.PropertyType));
                }
            }
        }
    }

    private static Func<ICommandExecutor, TCommand, Task> CreateTypedDeleteCommandDelegate<TCommand>()
    {
        return (ICommandExecutor commandExecutor, [AsParameters] TCommand query) =>
        {
            return commandExecutor.ExecuteAsync(query, CancellationToken.None);
        };
    }

    private static Func<IQueryExecutor, TQuery, Task<TResult>> CreateTypedQueryDelegate<TQuery, TResult>() where TQuery : IQuery<TResult>
    {
        return (IQueryExecutor queryExecutor, [AsParameters] TQuery query) => queryExecutor.ExecuteAsync(query, CancellationToken.None);
    }
}
