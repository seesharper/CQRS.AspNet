namespace CQRS.AspNet;

public record ParameterInfo(
    string Name,
    Type Type,
    string Description,
    bool IsOptional,
    string? Constraint,
    ParameterSource Source
);

public enum ParameterSource
{
    Route,
    Query
}
