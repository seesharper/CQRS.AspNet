using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;

namespace CQRS.AspNet.Example;

public record SampleQuery(string Name, int Age = 20) : IQuery<SampleQueryResult>;

public record SampleQueryResult(string Name, string Address);

public class FromQueryParametersQueryHandler : IQueryHandler<SampleQuery, SampleQueryResult>
{
    public Task<SampleQueryResult> HandleAsync(SampleQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SampleQueryResult(query.Name, "123 Main St."));
    }
}

[Post("/query-as-post")]
public record QueryAsPost(string Name, int Age = 20) : IQuery<SampleQueryResult>;

public class QueryAsPostHandler : IQueryHandler<QueryAsPost, SampleQueryResult>
{
    public Task<SampleQueryResult> HandleAsync(QueryAsPost query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SampleQueryResult(query.Name, "123 Main St."));
    }
}