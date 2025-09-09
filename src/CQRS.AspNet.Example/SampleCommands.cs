using System.ComponentModel;
using CQRS.AspNet.MetaData;
using CQRS.Command.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet.Example;

public record SampleCommand(int Id, string Name, string Address, int Age);

[Post("/post-command-with-result", Description = "This command returns a Created result.", Tags = ["Commands", "Example"])]
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

[Delete("/delete-command-with-result/{id}", Tags = ["Commands", "Delete", "Management"])]
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
[FromParameters]
public record PostCommandWithoutBody(int Id);


[FromParameters]
[Post("/post-command-without-body-with-result/{Id}")]
public record PostCommandWithoutBodyWithResult(int Id) : PostCommand<int>;


public class PostCommandWithoutBodyWithResultHandler : ICommandHandler<PostCommandWithoutBodyWithResult>
{
    public Task HandleAsync(PostCommandWithoutBodyWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created("post-command-without-body-with-result", command.Id));
        return Task.CompletedTask;
    }
}

// PostReportCommand => PostReport[Command]

[Post("/post-command-with-guid-parameter/{Id}", Description = "This command accepts a Guid parameter.", Name = "PostCommandWithGuidParameterId")]
public record PostCommandWithGuidParameter([Description("This is the Guid parameter")] Guid Id, string Value) : PostCommand<Guid>;

[Post("api/sample-post-command/{Id}")]
public record SamplePostCommand(int? Id, string Name, int Age = 20) : PostCommand;

public class SamplePostCommandHandler : ICommandHandler<SamplePostCommand>
{
    public Task HandleAsync(SamplePostCommand command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created());
        return Task.CompletedTask;
    }
}



[Post("api/sample-post-command-with-value-type/{Id}")]
public record SamplePostCommandWithValueType(int Id, string Name, int Age = 20) : PostCommand;

public class SamplePostCommandWithValueTypeHandler : ICommandHandler<SamplePostCommandWithValueType>
{
    public Task HandleAsync(SamplePostCommandWithValueType command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created());
        return Task.CompletedTask;
    }
}




// [Post("api/sample-post-command-with-invalid-property/{Id}")]
// public record SamplePostCommandWithInvalidProperty(int CustomerId, string Name, int Age = 20) : PostCommand;

[Post("api/sample-post-command-with-result/{Id}")]
public record SamplePostCommandWithResult(int Id) : PostCommand<int>;

public class SamplePostCommandFromBaseHandler : ICommandHandler<SamplePostCommandWithResult>
{
    public Task HandleAsync(SamplePostCommandWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created("sample-post-command-from-base", command.Id));
        return Task.CompletedTask;
    }
}

[Post("api/sample-post-command-with-problem/{Id}")]
public record SamplePostCommandWithProblem(int Id) : PostCommand;


public class SamplePostCommandWithProblemHandler : ICommandHandler<SamplePostCommandWithProblem>
{
    public Task HandleAsync(SamplePostCommandWithProblem command, CancellationToken cancellationToken = default)
    {
        command.SetProblemResult("Sample problem detail", "Sample instance", 500, "Sample title", "Sample type");
        return Task.CompletedTask;
    }
}


[Patch("api/sample-patch-command/{Id}")]
public record SamplePatchCommand(int Id) : PatchCommand;

public class SamplePatchCommandHandler : ICommandHandler<SamplePatchCommand>
{
    public Task HandleAsync(SamplePatchCommand command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.NoContent());
        return Task.CompletedTask;
    }
}

[Delete("api/sample-delete-command-from-base/{Id}")]
public record SampleDeleteCommandFromBase(int Id) : DeleteCommand;

public class SampleDeleteCommandFromBaseHandler : ICommandHandler<SampleDeleteCommandFromBase>
{
    public Task HandleAsync(SampleDeleteCommandFromBase command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.NoContent());
        return Task.CompletedTask;
    }
}

[Post("api/sample-post-command-with-body/{Id}")]
public record SamplePostCommandWithBody(int Id, string Name, int Age = 20) : PostCommand
{

}

public class SamplePostCommandWithBodyHandler : ICommandHandler<SamplePostCommandWithBody>
{
    public Task HandleAsync(SamplePostCommandWithBody command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created());
        return Task.CompletedTask;
    }
}


public interface IKeyedService
{
}

public class KeyedService : IKeyedService
{

}







public class AnotherKeyedService : IKeyedService
{
}

