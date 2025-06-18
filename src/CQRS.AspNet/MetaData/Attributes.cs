using System.Diagnostics.CodeAnalysis;

namespace CQRS.AspNet.MetaData;

[AttributeUsage(AttributeTargets.Class)]
public abstract class RouteBaseAttribute([StringSyntax("Route")] string route) : Attribute
{
    public string Route { get; } = route;

    public string Description { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;



    public RouteMetaData ToMetaData() => new(Route, Description, Summary);
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
