using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet;

public record PostCommand : ProblemCommand<Created>;

public record PostCommand<TValue> : ProblemCommand<Created<TValue>>;

public record PatchCommand : ProblemCommand<NoContent>;

public record DeleteCommand : ProblemCommand<NoContent>;