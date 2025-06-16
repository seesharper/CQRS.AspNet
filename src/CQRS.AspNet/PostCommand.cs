using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet;

public record PostCommand : ProblemCommand<Created>;

public record PostCommand<TValue> : ProblemCommand<Created<TValue>>;

public record PatchCommand : ProblemCommand<NoContent>;

public record DeleteCommand : ProblemCommand<NoContent>;

public class HttpResult<TValue>(IResult wrappedResult) : IResult, IValueHttpResult<TValue>
{
    public TValue? Value => throw new NotImplementedException();

    public Task ExecuteAsync(HttpContext httpContext)
    {
        return wrappedResult.ExecuteAsync(httpContext);
    }   
}

public class HttpResult2<TResult>(TResult wrappedResult) : IResult where TResult : IResult
{    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return wrappedResult.ExecuteAsync(httpContext);
    }   

    public static implicit operator HttpResult2<TResult>(TResult result)
    {
        return new HttpResult2<TResult>(result);
    }
}





public class HttpResult(IResult wrappedResult) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return wrappedResult.ExecuteAsync(httpContext);
    }

    // public static implicit operator Results<HttpResult, ProblemHttpResult>(Ok result)
}




public static class HttpResults
{
    public static HttpResult<Ok<TValue>> Ok<TValue>(TValue value)
    {
        return new HttpResult<Ok<TValue>>(TypedResults.Ok(value));
    }
}




public record GetQuery<TValue> : IQuery<Results<HttpResult<TValue>, ProblemHttpResult>>
{

}

public record GetQuery2<TResult> : IQuery<Results<TResult, ProblemHttpResult>> where TResult : IResult
{
    // This is a generic query that can return any result type that implements IResult.
    // It allows for more flexibility in the types of results that can be returned.
}

public record MyQuery : GetQuery<int>;


public class MyGetQueryHandler : IQueryHandler<MyQuery, Results<HttpResult<int>, ProblemHttpResult>>
{
    public async Task<Results<HttpResult<int>, ProblemHttpResult>> HandleAsync(MyQuery query, CancellationToken cancellationToken = default)
    {
        var t = TypedResults.Ok(42);
        return t;
        throw new NotImplementedException();
        Results<Ok<int>, BadRequest> result = TypedResults.Ok(42);
        Results<HttpResult, ProblemHttpResult> result2 = new HttpResult(TypedResults.Ok(42));
        Results<HttpResult<int>, ProblemHttpResult> result3 = new HttpResult<int>(TypedResults.Ok(42));
        return result3;


        var test = TypedResults.Ok(new HttpResult<int>(Results.Ok(42)));
    }
}

