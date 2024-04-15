using CQRS.AspNet.MetaData;
using CQRS.Command.Abstractions;
namespace CQRS.AspNet.Example;

[Post("/sample-query{id}")]
public record SampleCommand(int id, string Name, string Address, int Age);

public class SampleCommandHandler : ICommandHandler<SampleCommand>
{
    public Task HandleAsync(SampleCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}