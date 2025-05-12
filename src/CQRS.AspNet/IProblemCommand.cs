using CQRS.Command.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet;

public interface IProblemCommand
{
    void SetProblemResult(string? detail = null, string? instance = null, int? statusCode = null, string? title = null, string? type = null, IDictionary<string, object?>? extensions = null);

    bool HasProblemResult { get; }
}


public record ProblemCommand<TResult> : Command<Results<TResult, ProblemHttpResult>>, IProblemCommand where TResult : IResult
{
    public bool HasProblemResult { get; private set; }

    public void SetProblemResult(string? detail = null, string? instance = null, int? statusCode = null, string? title = null, string? type = null, IDictionary<string, object?>? extensions = null)
    {
        SetResult(TypedResults.Problem(
            detail: detail,
            instance: instance,
            statusCode: statusCode,
            title: title,
            type: type,
            extensions: extensions));
        HasProblemResult = true;
    }
}