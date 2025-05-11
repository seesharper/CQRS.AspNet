namespace CQRS.AspNet.Tests;

public static class ObjectExtensions
{
    public static string ToJson(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }
}