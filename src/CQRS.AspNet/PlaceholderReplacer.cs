namespace CQRS.AspNet;


using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

public static partial class PlaceholderReplacer
{
    public static string ReplacePlaceholders(string template, object values)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);

        var type = values.GetType();
        return MyRegex().Replace(template, match =>
        {
            var propertyName = match.Groups[1].Value;
            var property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found on object of type '{type.Name}'.");
            }

            var value = property.GetValue(values);
            if (value is null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' on object of type '{type.Name}' is null.");
            }
            return value.ToString()!;
        });
    }

    // Okay now I need a version of ReplacePlaceholders that does the same thing but
    // the remaining properties that are not in the template should be added to the template as query parameters
    // e.g. /api/user/{Id}?name={Name}&age={Age}
    // Remember that the template does not necessarily have to contain any placeholders

    public static string ReplacePlaceholdersWithQueryParameters(string template, object values)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);

        var type = values.GetType();
        var queryParameters = new List<string>();

        var result = MyRegex().Replace(template, match =>
        {
            var propertyName = match.Groups[1].Value;
            var property = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found on object of type '{type.Name}'.");
            }

            var value = property.GetValue(values);
            if (value is null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' on object of type '{type.Name}' is null.");
            }

            return value.ToString()!;
        });

        foreach (var property in type.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public))
        {
            if (!template.Contains($"{{{property.Name}}}", StringComparison.OrdinalIgnoreCase))
            {
                var value = property.GetValue(values);
                if (value != null)
                {
                    if (value is DateTime dateTime)
                    {
                        value = dateTime.ToString("o"); // ISO 8601 format
                    }
                    // else if (value is Guid guid)
                    // {
                    //     value = guid.ToString();
                    // }
                    // else if (value is IEnumerable enumerable)
                    // {
                    //     value = string.Join(",", enumerable.Cast<object>().Select(v => v.ToString()));
                    // }
                    queryParameters.Add($"{property.Name}={value}");
                }
            }
        }

        if (queryParameters.Count > 0)
        {
            result += "?" + string.Join("&", queryParameters);
        }

        return result;
    }


    [GeneratedRegex(@"\{([^}]+)\}")]
    private static partial Regex MyRegex();
}