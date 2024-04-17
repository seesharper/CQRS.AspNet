using System.Text.Json.Serialization;
using CQRS.AspNet.MetaData;
using CQRS.Command.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet.Example;

public record SampleCommand(int id, string Name, string Address, int Age);

[Post("/command-with-result")]
public record CommandWithResult(int Id) : Command<Results<ProblemHttpResult, Created>>;

public class CommandWithResultHandler : ICommandHandler<CommandWithResult>
{
    public async Task HandleAsync(CommandWithResult command, CancellationToken cancellationToken = default)
    {
        command.SetResult(TypedResults.Created());
    }
}

[Post("/command-without-setting-result")]
public record CommandWithoutSettingResult(int Id) : Command<Results<ProblemHttpResult, Created>>;

public class CommandWithoutSettingResultHandler : ICommandHandler<CommandWithoutSettingResult>
{
    public Task HandleAsync(CommandWithoutSettingResult command, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
