using CQRS.Command.Abstractions;

namespace CQRS.AspNet.Example;

public record DeleteCommand(int Id);
