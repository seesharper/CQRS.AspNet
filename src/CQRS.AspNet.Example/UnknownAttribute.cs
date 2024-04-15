using System.Diagnostics.CodeAnalysis;
using CQRS.AspNet.MetaData;

namespace CQRS.AspNet.Example;

public class UnknownRouteAttribute : RouteBaseAttribute
{
    public UnknownRouteAttribute([StringSyntax("Route")] string route) : base(route)
    {
    }
}