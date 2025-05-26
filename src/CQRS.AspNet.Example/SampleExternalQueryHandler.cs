using CQRS.AspNet.MetaData;
using CQRS.Query.Abstractions;
using LightInject.Microsoft.DependencyInjection;

namespace CQRS.AspNet.Example;

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ProductData Data { get; set; }
}

public class ProductData
{
    public string Color { get; set; }
    public string Capacity { get; set; }
}

[Get("objects")]
public record SampleExternalQuery : IQuery<IEnumerable<Product>>;


public class SampleExternalQueryHandler([FromKeyedServices("RestfulClient")] HttpClient httpClient) : IQueryHandler<SampleExternalQuery, IEnumerable<Product>>
{
    public async Task<IEnumerable<Product>> HandleAsync(SampleExternalQuery query, CancellationToken cancellationToken)
    {
        return await httpClient.Get(query, cancellationToken: cancellationToken);        
    }
}
