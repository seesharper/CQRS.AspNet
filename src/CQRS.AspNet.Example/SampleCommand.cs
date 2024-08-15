using System.Text.Json.Serialization;
using CQRS.AspNet.MetaData;
using CQRS.Command.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet.Example;

public record SampleCommand(int Id, string Name, string Address, int Age);

[Post("/post-command-with-result")]
public record PostCommandWithResult(int Id) : Command<Results<ProblemHttpResult, Created>>;

public class PostCommandWithResultHandler : ICommandHandler<PostCommandWithResult>
{
    public Task HandleAsync(PostCommandWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created());
        return Task.CompletedTask;
    }
}

[Patch("/patch-command-with-result")]
public record PatchCommandWithResult(int Id) : Command<Results<ProblemHttpResult, NoContent>>;

public class PatchCommandWithResultHandler : ICommandHandler<PatchCommandWithResult>
{
    public Task HandleAsync(PatchCommandWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.NoContent());
        return Task.CompletedTask;
    }
}

[Put("/put-command-with-result")]
public record PutCommandWithResult(int Id) : Command<Results<ProblemHttpResult, NoContent>>;

public class PutCommandWithResultHandler : ICommandHandler<PutCommandWithResult>
{
    public Task HandleAsync(PutCommandWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.NoContent());
        return Task.CompletedTask;
    }
}

[Delete("/delete-command-with-result/{id}")]
public record DeleteCommandWithResult(int Id) : Command<Results<ProblemHttpResult, Ok<DeleteCommandResult>>>;

public class DeleteCommandWithResultHandler : ICommandHandler<DeleteCommandWithResult>
{
    public Task HandleAsync(DeleteCommandWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Ok(new DeleteCommandResult(command.Id)));
        return Task.CompletedTask;
    }
}

public record DeleteCommandResult(int Id);


[Post("/command-without-setting-result")]
public record CommandWithoutSettingResult(int Id) : Command<Results<ProblemHttpResult, Created>>;

public class CommandWithoutSettingResultHandler : ICommandHandler<CommandWithoutSettingResult>
{
    public Task HandleAsync(CommandWithoutSettingResult command, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public record CreateCommand : Command<Results<Created<int>, ProblemHttpResult>>;

[Post("/command-inheriting-from-create-command")]
public record CommandInheritingFromCreateCommand : CreateCommand;

public class CommandInheritingFromCreateCommandHandler : ICommandHandler<CommandInheritingFromCreateCommand>
{
    public Task HandleAsync(CommandInheritingFromCreateCommand command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created("command-inheriting-from-create-command", 1));
        return Task.CompletedTask;
    }
}


[Post("/post-command-without-body/{Id}")]
public record PostCommandWithoutBody(int Id);

