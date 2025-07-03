namespace CQRS.AspNet;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public static class RouteHelper
{
    private static readonly Regex RouteParamRegex = new(@"{(.*?)}", RegexOptions.Compiled);

    public static List<RouteParameterInfo> ExtractRouteParameters(string routeTemplate, Type targetType)
    {
        var matches = RouteParamRegex.Matches(routeTemplate)
            .Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .ToList();

        var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        // Get constructor parameters and map by name for description lookup
        var constructor = targetType.GetConstructors()
                                    .OrderByDescending(c => c.GetParameters().Length)
                                    .FirstOrDefault();

        var constructorParams = constructor?
            .GetParameters()
            .ToDictionary(p => p.Name!, p => p, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, ParameterInfo>(StringComparer.OrdinalIgnoreCase);

        var result = new List<RouteParameterInfo>();

        foreach (var token in matches)
        {
            var isOptional = token.Contains('?');
            var constraint = token.Contains(':') ? token.Split(':', 2)[1].TrimEnd('?') : null;
            var paramName = token.Split(new[] { ':', '?' }, 2)[0];

            if (!properties.TryGetValue(paramName, out var property))
            {
                throw new InvalidOperationException(
                    $"Route parameter '{paramName}' does not match any property in type '{targetType.Name}'.");
            }

            // Try to get description from property or constructor parameter
            var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description
                             ?? (constructorParams.TryGetValue(paramName, out var ctorParam)
                                    ? ctorParam.GetCustomAttribute<DescriptionAttribute>()?.Description
                                    : null)
                             ?? string.Empty;

            result.Add(new RouteParameterInfo(
                Name: property.Name,
                Type: property.PropertyType,
                Description: description,
                IsOptional: isOptional,
                Constraint: constraint
            ));
        }

        return result;
    }
}
