using System.Reflection;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;

namespace CQRS.AspNet;

/// <summary>
/// A wrapper that can hold any IValueHttpResult&lt;TValue&gt; implementation.
/// </summary>
/// <typeparam name="TValue">The type of value contained in the result.</typeparam>
public class AnyResult<TValue> : IResult, IValueHttpResult, IValueHttpResult<TValue>, IEndpointMetadataProvider, IStatusCodeHttpResult
{
    private readonly IResult _innerResult;

    public AnyResult(IValueHttpResult<TValue> innerResult)
    {
        if (innerResult is not IResult result)
            throw new ArgumentException($"The provided result must implement {nameof(IResult)}", nameof(innerResult));

        _innerResult = result;
    }

    public TValue? Value => ((IValueHttpResult<TValue>)_innerResult).Value;

    public int? StatusCode => ((IStatusCodeHttpResult)_innerResult).StatusCode;

    object? IValueHttpResult.Value => ((IValueHttpResult)_innerResult).Value;

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        return _innerResult.ExecuteAsync(httpContext);
    }
    public static implicit operator AnyResult<TValue>(Created<TValue> created) => new(created);
    public static implicit operator AnyResult<TValue>(Ok<TValue> ok) => new(ok);
    public static implicit operator AnyResult<TValue>(Accepted<TValue> accepted) => new(accepted);
    public static implicit operator AnyResult<TValue>(CreatedAtRoute<TValue> createdAtRoute) => new(createdAtRoute);
    public static implicit operator AnyResult<TValue>(AcceptedAtRoute<TValue> acceptedAtRoute) => new(acceptedAtRoute);

    // Add more implicit operators as needed for other IValueHttpResult<TValue> implementations
}

public record PostCommand : ProblemCommand<IResult>;

public record PostCommand<TValue> : ProblemCommand<AnyResult<TValue>>;

public record PatchCommand : ProblemCommand<IResult>;

public record DeleteCommand : ProblemCommand<IResult>;

public record GetQuery<TValue> : IQuery<Results<Ok<TValue>, ProblemHttpResult>>;


/// <summary>
/// Extension methods to enable direct assignment of IValueHttpResult types to commands.
/// </summary>
public static class ProblemCommandExtensions
{
    /// <summary>
    /// Sets the result for a ProblemCommand that expects AnyResult&lt;TValue&gt;.
    /// </summary>
    public static void SetResult<TValue>(this ProblemCommand<AnyResult<TValue>> command, Created<TValue> result)
        => command.SetResult((AnyResult<TValue>)result);

    /// <summary>
    /// Sets the result for a ProblemCommand that expects AnyResult&lt;TValue&gt;.
    /// </summary>
    public static void SetResult<TValue>(this ProblemCommand<AnyResult<TValue>> command, Ok<TValue> result)
        => command.SetResult((AnyResult<TValue>)result);

    /// <summary>
    /// Sets the result for a ProblemCommand that expects AnyResult&lt;TValue&gt;.
    /// </summary>
    public static void SetResult<TValue>(this ProblemCommand<AnyResult<TValue>> command, Accepted<TValue> result)
        => command.SetResult((AnyResult<TValue>)result);

    /// <summary>
    /// Sets the result for a ProblemCommand that expects AnyResult&lt;TValue&gt;.
    /// </summary>
    public static void SetResult<TValue>(this ProblemCommand<AnyResult<TValue>> command, CreatedAtRoute<TValue> result)
        => command.SetResult((AnyResult<TValue>)result);

    /// <summary>
    /// Sets the result for a ProblemCommand that expects AnyResult&lt;TValue&gt;.
    /// </summary>
    public static void SetResult<TValue>(this ProblemCommand<AnyResult<TValue>> command, AcceptedAtRoute<TValue> result)
        => command.SetResult((AnyResult<TValue>)result);
}




