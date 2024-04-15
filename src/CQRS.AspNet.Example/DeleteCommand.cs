using CQRS.Command.Abstractions;

namespace CQRS.AspNet.Example;

public record DeleteCommand(int id);

public class DeleteCommandHandler : ICommandHandler<DeleteCommand>
{
    public Task HandleAsync(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}