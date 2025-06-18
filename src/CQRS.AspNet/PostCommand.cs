using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet;

public record PostCommand : ProblemCommand<Created>;

public record PostCommand<TValue> : ProblemCommand<Created<TValue>>;

public record PatchCommand : ProblemCommand<NoContent>;

public record DeleteCommand : ProblemCommand<NoContent>;

public record GetQuery<TValue> : IQuery<Results<Ok<TValue>, ProblemHttpResult>>;



