using CQRS.AspNet.MetaData;
using Microsoft.AspNetCore.Builder;

namespace CQRS.AspNet;

/// <summary>
/// Extension methods for RouteHandlerBuilder to support metadata configuration.
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Applies metadata from a RouteMetaData object to the RouteHandlerBuilder.
    /// </summary>
    /// <param name="builder">The RouteHandlerBuilder to configure.</param>
    /// <param name="metaData">The RouteMetaData containing the metadata to apply.</param>
    /// <returns>The configured RouteHandlerBuilder.</returns>
    public static RouteHandlerBuilder WithMetadata(this RouteHandlerBuilder builder, RouteMetaData metaData)
    {
        if (!string.IsNullOrEmpty(metaData.Description))
        {
            builder.WithDescription(metaData.Description);
        }
        if (!string.IsNullOrEmpty(metaData.Summary))
        {
            builder.WithSummary(metaData.Summary);
        }
        if (!string.IsNullOrEmpty(metaData.Name))
        {
            builder.WithName(metaData.Name);
        }
        if (metaData.ExcludeFromDescription)
        {
            builder.ExcludeFromDescription();
        }
        return builder;
    }
}
