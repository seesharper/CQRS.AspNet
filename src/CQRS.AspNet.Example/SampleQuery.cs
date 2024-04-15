using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;

namespace CQRS.AspNet.Example;

[Get("/sample-query")]
public record SampleQuery(string Name, int Age = 20) : IQuery<SampleQueryResult>;

public record SampleQueryResult(string Name, string Address);

public class FromQueryParametersQueryHandler : IQueryHandler<SampleQuery, SampleQueryResult>
{
    public Task<SampleQueryResult> HandleAsync(SampleQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SampleQueryResult(query.Name, "123 Main St."));
    }
}