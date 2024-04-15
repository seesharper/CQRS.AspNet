using System.Diagnostics.CodeAnalysis;

namespace CQRS.AspNet.Example;

[ExcludeFromCodeCoverage]
[UnknownRoute("UnknownRoute")]
public record CommandWithUnknownAttribute();