namespace CQRS.AspNet.MetaData;


public record RouteMetaData(
    string Route,
    string Description = "",
    string Summary = ""
);