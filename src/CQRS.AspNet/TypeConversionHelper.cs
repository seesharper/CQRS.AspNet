using System;
using System.ComponentModel;

public static class TypeConversionHelper
{
    public static object ConvertTo(string input, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            if (IsNullable(targetType))
                return null;

            throw new InvalidOperationException("Cannot convert null or whitespace to a non-nullable type.");
        }

        // Handle nullable types
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Use TypeConverter for better flexibility
        var converter = TypeDescriptor.GetConverter(underlyingType);
        if (converter != null && converter.IsValid(input))
        {
            return converter.ConvertFromInvariantString(input);
        }

        // Fallback to Convert.ChangeType
        return Convert.ChangeType(input, underlyingType);
    }

    private static bool IsNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }
}
