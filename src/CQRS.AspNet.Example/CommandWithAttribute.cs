using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;

namespace CQRS.AspNet.Example;

[Post("/command-with-attribute")]
[Put("/command-with-attribute/{Id}")]
[Patch("/command-with-attribute/{Id}")]
public record CommandWithAttribute(int Id, string Name, string Address, int Age);

[Delete("/delete-command-with-attribute/{Id}")]
public record DeleteCommandWithAttribute(int Id);

[Get("/query-with-attribute/{Id}")]
public record QueryWithAttribute(int Id) : IQuery<QueryWithAttributeResult>;

public record QueryWithAttributeResult(string Name, string Address);