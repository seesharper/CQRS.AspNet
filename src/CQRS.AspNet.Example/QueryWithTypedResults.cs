using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CQRS.AspNet.Example;

[Get("/query-with-typed-results/{Id}")]
public record QueryWithTypedResults(int Id) : IQuery<Results<Ok<int>, NoContent>>;

public class QueryWithTypedResultsHandler : IQueryHandler<QueryWithTypedResults, Results<Ok<int>, NoContent>>
{
    public async Task<Results<Ok<int>, NoContent>> HandleAsync(QueryWithTypedResults query, CancellationToken cancellationToken)
    {
        if (query.Id == 1)
        {
            return TypedResults.Ok(2);
        }

        return TypedResults.NoContent();
    }
}