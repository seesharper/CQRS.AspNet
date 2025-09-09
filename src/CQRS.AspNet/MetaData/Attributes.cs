using System.Diagnostics.CodeAnalysis;

namespace CQRS.AspNet.MetaData;

[AttributeUsage(AttributeTargets.Class)]
public abstract class RouteBaseAttribute([StringSyntax("Route")] string route) : Attribute
{
    /// <summary>
    /// The route pattern associated with the command or query (e.g. "customers/{id}").
    /// </summary>
    public string Route { get; } = route;

    /// <summary>
    /// A longer descriptive text explaining the purpose and behavior of the endpoint.
    /// Appears in generated API documentation (e.g. OpenAPI) as the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// A short one-line summary for the endpoint used in documentation UIs.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// An explicit endpoint name (used for link generation and display). When empty a name may be inferred.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When set to true the endpoint will be hidden from generated descriptions (e.g. excluded from OpenAPI output / UI).
    /// </summary>
    public bool ExcludeFromDescription { get; set; }

    /// <summary>
    /// Tags to categorize the endpoint in API documentation (e.g., OpenAPI/Swagger).
    /// </summary>
    public string[] Tags { get; set; } = [];

    public RouteMetaData ToMetaData() => new(Route, Description, Summary, Name, ExcludeFromDescription, Tags);
};

public class GetAttribute([StringSyntax("Route")] string route)
    : RouteBaseAttribute(route);

public class PostAttribute([StringSyntax("Route")] string route)
    : RouteBaseAttribute(route);

public class DeleteAttribute([StringSyntax("Route")] string route)
    : RouteBaseAttribute(route);

public class PutAttribute([StringSyntax("Route")] string route)
    : RouteBaseAttribute(route);

public class PatchAttribute([StringSyntax("Route")] string route)
    : RouteBaseAttribute(route);

[AttributeUsage(AttributeTargets.Class)]
public class FromParameters : Attribute;
