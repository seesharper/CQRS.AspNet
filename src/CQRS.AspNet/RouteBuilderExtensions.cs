﻿using System.Reflection;
using CQRS.AspNet.MetaData;
using CQRS.Command.Abstractions;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace CQRS.AspNet;

public static class RouteBuilderExtensions
{
    private static readonly MethodInfo CreateTypedQueryDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedQueryDelegate), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
    private static readonly MethodInfo CreateTypedCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedCommandDelegate), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
    private static readonly MethodInfo CreateTypedDeleteCommandDelegateMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(CreateTypedDeleteCommandDelegate), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
    private static readonly MethodInfo MapGetMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapGet), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapPostMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPost), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapDeleteMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapDelete), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapPatchMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPatch), BindingFlags.Public | BindingFlags.Static)!;
    private static readonly MethodInfo MapPutMethod = typeof(RouteBuilderExtensions).GetMethod(nameof(MapPut), BindingFlags.Public | BindingFlags.Static)!;


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
                    _ => throw new InvalidOperationException("Invalid route attribute")
                };

                method.MakeGenericMethod(type).Invoke(null, [builder, routeAttribute.Route]);
            }
        }
        return builder;
    }

    public static RouteHandlerBuilder MapGet<TQuery>(this IEndpointRouteBuilder builder, string pattern)
    {
        var openGenericExecuteAsyncMethod = typeof(IQueryExecutor).GetMethod(nameof(IQueryExecutor.ExecuteAsync))!;
        var queryInterface = typeof(TQuery).GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
        var resultType = queryInterface.GetGenericArguments()[0];

        var createTypedDelegateMethod = CreateTypedQueryDelegateMethod.MakeGenericMethod(typeof(TQuery), resultType);
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapGet(pattern, typedDelegate);
    }

    public static RouteHandlerBuilder MapPost<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapPost(pattern, typedDelegate);
    }

    public static RouteHandlerBuilder MapDelete<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedDeleteCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapDelete(pattern, typedDelegate);
    }

    public static RouteHandlerBuilder MapPatch<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapPatch(pattern, typedDelegate);
    }

    public static RouteHandlerBuilder MapPut<TCommand>(this IEndpointRouteBuilder builder, string pattern)
    {
        var openGenericExecuteAsyncMethod = typeof(ICommandExecutor).GetMethod(nameof(ICommandExecutor.ExecuteAsync))!;

        var createTypedDelegateMethod = CreateTypedCommandDelegateMethod.MakeGenericMethod(typeof(TCommand));
        var typedDelegate = (Delegate)createTypedDelegateMethod.Invoke(null, null)!;

        return builder.MapPut(pattern, typedDelegate);
    }

    private static Func<HttpRequest, ICommandExecutor, TCommand, Task> CreateTypedCommandDelegate<TCommand>()
    {
        return (HttpRequest request, ICommandExecutor commandExecutor, [FromBody] TCommand query) =>
        {
            var routeValues = request.RouteValues;
            if (routeValues.Count > 0)
            {
                foreach (var routeValue in routeValues)
                {
                    var property = typeof(TCommand).GetProperty(routeValue.Key, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        property.SetValue(query, Convert.ChangeType(routeValue.Value, property.PropertyType));
                    }
                }
            }
            return commandExecutor.ExecuteAsync(query, CancellationToken.None);
        };
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