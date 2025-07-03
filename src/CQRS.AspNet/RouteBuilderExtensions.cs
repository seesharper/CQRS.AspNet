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

    private static readonly MethodInfo CreateParameterizedTypedCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateParameterizedTypedCommandDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CreateParameterizedTypedCommandDelegateWithResultMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateParameterizedTypedCommandDelegateWithResult), BindingFlags.NonPublic | BindingFlags.Static)!;


    private static readonly MethodInfo CreateTypedDeleteCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedDeleteCommandDelegate), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CreateTypedCommandDelegateWithResultMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedCommandDelegateWithResult), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo CreateTypedDeleteCommandDelegateWithResultMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedDeleteCommandDelegateWithResult), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo MapGetMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapGetInternal), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo MapPostMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPostInternal), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo MapDeleteMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapDeleteInternal), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo MapPatchMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPatchInternal), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo MapPutMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPutInternal), BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>
    /// Maps command and queries that are decorated with <see cref="GetAttribute"/>, <see cref="PostAttribute"/>, <see cref="DeleteAttribute"/>, <see cref="PatchAttribute"/>, <see cref="PutAttribute"/> attributes.
    /// </summary>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="assembly">The <see cref="Assembly"/> for which to scan for commands and queries</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining calls. </returns>
    public static IEndpointRouteBuilder MapCqrsEndpoints(this IEndpointRouteBuilder builder, Assembly? assembly = null)
    {
        var allTypes = assembly?.GetTypes() ?? Assembly.GetCallingAssembly()!.GetTypes();
        var typeWithRouteAttribute = allTypes.Where(t => t.IsPublic && t.GetCustomAttributes<RouteBaseAttribute>().Any());
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

                method?.MakeGenericMethod(type).Invoke(null, [builder, routeAttribute.ToMetaData()]);
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
        return MapGetInternal<TQuery>(builder, new RouteMetaData(pattern));
    }

    /// <summary>
    /// Maps the given <typeparamref name="TQuery"/> to the specified GET route <paramref name="pattern"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to be mapped to a GET endpoint.</typeparam>
    /// <param name="builder">The target <see cref="IEndpointRouteBuilder"/>.</param>    
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.</returns>
    public static RouteHandlerBuilder MapGet<TQuery>(this IEndpointRouteBuilder builder)
    {
        var getAttributes = typeof(TQuery).GetCustomAttributes<GetAttribute>();
        if (!getAttributes.Any())
        {
            throw new InvalidOperationException($"Type {typeof(TQuery).Name} does not have a GetAttribute defined. Use MapGet<TQuery>(IEndpointRouteBuilder, string) to specify a route.");
        }

        RouteHandlerBuilder? routeHandlerBuilder = null;

        foreach (var getAttribute in getAttributes)
        {
            var pattern = getAttribute.Route;
            if (getAttribute.Description != null)
            {
                getAttribute.Description = string.Empty;
            }
            routeHandlerBuilder = MapGetInternal<TQuery>(builder, getAttribute.ToMetaData());
        }

        return routeHandlerBuilder!;
    }

    private static RouteHandlerBuilder MapGetInternal<TQuery>(IEndpointRouteBuilder builder, RouteMetaData metaData)
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

        return builder.MapGet(metaData.Route, typedDelegate).WithDescription(metaData.Description)
            .WithSummary(metaData.Summary);
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
        return MapPostInternal<TCommandOrQuery>(builder, new RouteMetaData(pattern));
    }

    private static RouteHandlerBuilder MapPostInternal<TCommandOrQuery>(IEndpointRouteBuilder builder, RouteMetaData metaData)
    {
        if (typeof(TCommandOrQuery).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
        {
            var queryInterface = typeof(TCommandOrQuery).GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IQuery<>));
            var resultType = queryInterface.GetGenericArguments()[0];

            var createTypedDelegateMethod = CreateTypedQueryDelegateForPostMethod.MakeGenericMethod(typeof(TCommandOrQuery), resultType);
            var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;
            return builder.MapPost(metaData.Route, typedDelegate);
        }

        var parametersType = CreateParameterType(metaData, typeof(TCommandOrQuery));
        var parameterizedTypedDelegateMethod = GetParameterizedTypedDelegateMethod(typeof(TCommandOrQuery), parametersType);
        // var createParameterizedTypedDelegateMethod = CreateParameterizedTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommandOrQuery), parametersType);
        return builder.MapPost(metaData.Route, (Delegate)parameterizedTypedDelegateMethod.Invoke(null, null)!)
            .WithDescription(metaData.Description)
            .WithSummary(metaData.Summary);

        // return builder.MapPost(metaData.Route, (Delegate)GetCreateTypedDelegateMethod<TCommandOrQuery>().Invoke(null, null)!);
    }

    private static Type CreateParameterType(RouteMetaData routeMetaData, Type commandType)
    {
        var routeParameters = RouteHelper.ExtractRouteParameters(routeMetaData.Route, commandType);
        var parameterType = ParameterTypeBuilder.CreateParameterType($"{commandType.Name}Parameters", routeParameters);
        return parameterType;
    }


    private static MethodInfo GetParameterizedTypedDelegateMethod(Type commandType, Type parametersType)
    {
        var genericCommandType = GetGenericCommandType(commandType);
        if (genericCommandType != null)
        {
            var resultType = genericCommandType.GetGenericArguments()[0];
            return CreateParameterizedTypedCommandDelegateWithResultMethod.MakeGenericMethod(commandType, parametersType, resultType);
        }
        else
        {
            return CreateParameterizedTypedCommandDelegateMethod.MakeGenericMethod(commandType, parametersType);
        }
    }



    private static MethodInfo GetCreateTypedDelegateMethod<TCommand>()
    {
        var genericCommandType = GetGenericCommandType(typeof(TCommand));
        if (genericCommandType != null)
        {
            var resultType = genericCommandType.GetGenericArguments()[0];
            return CreateTypedCommandDelegateWithResultMethod.MakeGenericMethod(typeof(TCommand), resultType);
        }
        else
        {
            return CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        }
    }

    private static MethodInfo GetCreateTypedDeleteDelegateMethod<TCommand>()
    {
        var genericCommandType = GetGenericCommandType(typeof(TCommand));

        if (genericCommandType != null)
        {
            var resultType = genericCommandType.GetGenericArguments()[0];
            return CreateTypedDeleteCommandDelegateWithResultMethod.MakeGenericMethod(typeof(TCommand), resultType);
        }
        else
        {
            return CreateTypedDeleteCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        }
    }

    private static Type? GetGenericCommandType(Type type)
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
    private static Type? GetReturnType(Type type)
    {
        // Check for IQuery<TResult> interface
        var queryInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
        if (queryInterface != null)
        {
            return queryInterface.GetGenericArguments()[0];
        }

        // Check for Command<T> in inheritance hierarchy
        var currentType = type;
        while (currentType.BaseType != null && currentType.BaseType != typeof(object))
        {
            if (currentType.BaseType.IsGenericType && currentType.BaseType.GetGenericTypeDefinition() == typeof(Command<>))
            {
                return currentType.BaseType.GetGenericArguments()[0];
            }
            currentType = currentType.BaseType;
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
        return MapPatchInternal<TCommand>(builder, new RouteMetaData(pattern));
    }

    private static RouteHandlerBuilder MapPatchInternal<TCommand>(IEndpointRouteBuilder builder, RouteMetaData metaData)
    {
        return builder.MapPatch(metaData.Route, (Delegate)GetCreateTypedDelegateMethod<TCommand>().Invoke(null, null)!);
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
        return MapPutInternal<TCommand>(builder, new RouteMetaData(pattern));
    }

    private static RouteHandlerBuilder MapPutInternal<TCommand>(IEndpointRouteBuilder builder, RouteMetaData metaData)
    {
        return builder.MapPut(metaData.Route, (Delegate)GetCreateTypedDelegateMethod<TCommand>().Invoke(null, null)!);
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
        return MapDeleteInternal<TCommand>(builder, new RouteMetaData(pattern));
    }

    private static RouteHandlerBuilder MapDeleteInternal<TCommand>(IEndpointRouteBuilder builder, RouteMetaData metaData)
    {
        return builder.MapDelete(metaData.Route, (Delegate)GetCreateTypedDeleteDelegateMethod<TCommand>().Invoke(null, null)!);
    }

    private static Func<HttpRequest, ICommandExecutor, TParameter, TCommand, Task> CreateParameterizedTypedCommandDelegate<TCommand, TParameter>()
    {
        return async (HttpRequest request, ICommandExecutor commandExecutor, [AsParameters] TParameter parameters, [FromBody] TCommand command) =>
            {
                MapRouteValues(request, command);
                await commandExecutor.ExecuteAsync(command, CancellationToken.None);
            };
    }

    private static Func<HttpRequest, ICommandExecutor, TParameter, TCommand, Task<TResult>> CreateParameterizedTypedCommandDelegateWithResult<TCommand, TParameter, TResult>() where TCommand : Command<TResult>
    {
        return async (HttpRequest request, ICommandExecutor commandExecutor, [AsParameters] TParameter parameters, [FromBody] TCommand command) =>
            {
                MapRouteValues(request, command);
                await commandExecutor.ExecuteAsync(command, CancellationToken.None);
                return command.GetResult()!;
            };
    }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task> CreateTypedCommandDelegate<TCommand>()
    {
        if (typeof(TCommand).IsDefined(typeof(FromParameters)))
        {
            return async (HttpRequest request, ICommandExecutor commandExecutor, [AsParameters] TCommand command) =>
            {
                MapRouteValues(request, command);
                await commandExecutor.ExecuteAsync(command, CancellationToken.None);
            };
        }
        else
        {
            return async (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand command) =>
            {
                MapRouteValues(request, command);
                await commandExecutor.ExecuteAsync(command, CancellationToken.None);
            };
        }
    }

    // private static Func<HttpRequest, ICommandExecutor, TCommand, Task<TResult>> CreateTypedCommandDelegateWithResult<TCommand, TResult>() where TCommand : Command<TResult>
    // {

    // }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task<TResult>> CreateTypedCommandDelegateWithResult<TCommand, TResult>() where TCommand : Command<TResult>
    {
        if (typeof(TCommand).IsDefined(typeof(FromParameters)))
        {
            return async (HttpRequest request, ICommandExecutor commandExecutor, [AsParameters] TCommand command) =>
            {
                MapRouteValues(request, command);
                await commandExecutor.ExecuteAsync(command, CancellationToken.None);
                return command.GetResult()!;
            };
        }
        else
        {
            return async (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand command) =>
            {
                MapRouteValues(request, command);
                await commandExecutor.ExecuteAsync(command, CancellationToken.None);
                return command.GetResult()!;
            };
        }
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
                    property.SetValue(command, TypeConversionHelper.ConvertTo(routeValue.Value!.ToString()!, property.PropertyType));
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
