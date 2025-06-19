namespace CQRS.AspNet;

public record RouteParameterInfo(
    string Name,
    Type Type,
    string Description,
    bool IsOptional,
    string? Constraint
);