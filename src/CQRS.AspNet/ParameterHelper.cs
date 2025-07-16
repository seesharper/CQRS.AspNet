namespace CQRS.AspNet;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public static class ParameterHelper
{
    private static readonly Regex RouteParamRegex = new(@"{(.*?)}", RegexOptions.Compiled);

    public static List<ParameterInfo> ExtractRouteParameters(string routeTemplate, Type targetType)
    {
        string typeName = targetType.Name;

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
            ?? new Dictionary<string, System.Reflection.ParameterInfo>(StringComparer.OrdinalIgnoreCase);

        var result = new List<ParameterInfo>();

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

            result.Add(new ParameterInfo(
                Name: property.Name,
                Type: property.PropertyType,
                Description: description,
                IsOptional: isOptional,
                Constraint: constraint,
                Source: ParameterSource.Route
            ));
        }

        return result;
    }

    public static List<ParameterInfo> ExtractQueryParameters(Type targetType, params string[] excludePropertyNames)
    {
        var excludeSet = new HashSet<string>(excludePropertyNames, StringComparer.OrdinalIgnoreCase);

        var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(p => !excludeSet.Contains(p.Name))
                                   .ToList();

        // Get constructor parameters and map by name for description lookup
        var constructor = targetType.GetConstructors()
                                    .OrderByDescending(c => c.GetParameters().Length)
                                    .FirstOrDefault();

        var constructorParams = constructor?
            .GetParameters()
            .ToDictionary(p => p.Name!, p => p, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, System.Reflection.ParameterInfo>(StringComparer.OrdinalIgnoreCase);

        var result = new List<ParameterInfo>();

        foreach (var property in properties)
        {
            // Determine if the property is optional (nullable reference type or nullable value type)
            var isOptional = IsNullableType(property.PropertyType) ||
                           property.PropertyType.IsClass ||
                           property.PropertyType == typeof(string);

            // Try to get description from property or constructor parameter
            var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description
                             ?? (constructorParams.TryGetValue(property.Name, out var ctorParam)
                                    ? ctorParam.GetCustomAttribute<DescriptionAttribute>()?.Description
                                    : null)
                             ?? string.Empty;

            result.Add(new ParameterInfo(
                Name: property.Name,
                Type: property.PropertyType,
                Description: description,
                IsOptional: isOptional,
                Constraint: null, // Query parameters don't have constraints like route parameters
                Source: ParameterSource.Query
            ));
        }

        return result;
    }

    public static List<ParameterInfo> ExtractAllParameters(string routeTemplate, Type targetType)
    {
        var routeParams = ExtractRouteParameters(routeTemplate, targetType);
        var routeParamNames = routeParams.Select(p => p.Name).ToArray();
        var queryParams = ExtractQueryParameters(targetType, routeParamNames);

        var result = new List<ParameterInfo>();
        result.AddRange(routeParams);
        result.AddRange(queryParams);

        return result;
    }

    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }
}
