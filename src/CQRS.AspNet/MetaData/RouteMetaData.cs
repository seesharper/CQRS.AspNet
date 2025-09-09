namespace CQRS.AspNet.MetaData;


public record RouteMetaData(
    string Route,
    string? Description = null,
    string? Summary = null,
    string? Name = null,
    bool ExcludeFromDescription = false,
    string[]? Tags = null
);