namespace CQRS.AspNet.Tests;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using AwesomeAssertions;
using Xunit;

public class ParameterHelperTests
{
    public record TestCommand(
        [property: Description("The unique identifier")] Guid Id,
        [property: Description("Optional value")] string? OptionalValue,
        int Count,
        bool IsActive
    );

    public record TestQuery(
        [property: Description("The unique identifier")] Guid Id,
        [property: Description("The name to search for")] string? Name,
        [property: Description("Page number")] int Page = 1,
        [property: Description("Page size")] int PageSize = 10
    );

    [Fact]
    public void Should_extract_single_route_parameter_without_constraint()
    {
        var route = "/api/{Id}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, null, ParameterSource.Route));
    }

    [Fact]
    public void Should_extract_route_parameter_with_constraint()
    {
        var route = "/api/{Id:guid}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid", ParameterSource.Route));
    }

    [Fact]
    public void Should_extract_optional_route_parameter()
    {
        var route = "/api/{OptionalValue?}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new ParameterInfo(
            "OptionalValue", typeof(string), "Optional value", true, null, ParameterSource.Route));
    }

    [Fact]
    public void Should_extract_route_parameter_with_constraint_and_optional()
    {
        var route = "/api/{Count:int?}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new ParameterInfo(
            "Count", typeof(int), "", true, "int", ParameterSource.Route));
    }

    [Fact]
    public void Should_extract_multiple_route_parameters()
    {
        var route = "/api/{Id:guid}/{Count:int}/{IsActive:bool}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(3);
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid", ParameterSource.Route));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Count", typeof(int), "", false, "int", ParameterSource.Route));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "IsActive", typeof(bool), "", false, "bool", ParameterSource.Route));
    }

    [Fact]
    public void Should_throw_exception_for_missing_route_parameter()
    {
        var route = "/api/{NonExistentParam}";

        Action act = () => ParameterHelper.ExtractRouteParameters(route, typeof(TestCommand));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Route parameter 'NonExistentParam' does not match any property in type 'TestCommand'.");
    }

    [Fact]
    public void Should_extract_query_parameters()
    {
        var result = ParameterHelper.ExtractQueryParameters(typeof(TestQuery));

        result.Should().HaveCount(4);
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, null, ParameterSource.Query));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Name", typeof(string), "The name to search for", true, null, ParameterSource.Query));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Page", typeof(int), "Page number", false, null, ParameterSource.Query));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "PageSize", typeof(int), "Page size", false, null, ParameterSource.Query));
    }

    [Fact]
    public void Should_extract_query_parameters_excluding_specified_properties()
    {
        var result = ParameterHelper.ExtractQueryParameters(typeof(TestQuery), "Id", "Name");

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Page", typeof(int), "Page number", false, null, ParameterSource.Query));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "PageSize", typeof(int), "Page size", false, null, ParameterSource.Query));
    }

    [Fact]
    public void Should_extract_all_parameters_combining_route_and_query()
    {
        var route = "/api/{Id:guid}";
        var result = ParameterHelper.ExtractAllParameters(route, typeof(TestQuery));

        result.Should().HaveCount(4);

        // Should contain the route parameter
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid", ParameterSource.Route));

        // Should contain the query parameters (excluding Id since it's in route)
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Name", typeof(string), "The name to search for", true, null, ParameterSource.Query));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Page", typeof(int), "Page number", false, null, ParameterSource.Query));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "PageSize", typeof(int), "Page size", false, null, ParameterSource.Query));
    }

    public record TestCommandWithParamAttributes(
        [property: Description("The unique identifier")] Guid Id,
        [property: Description("Optional value")] string? OptionalValue,
        int Count
    );

    [Fact]
    public void Should_extract_route_parameter_with_description_from_property()
    {
        var route = "/api/{Id}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommandWithParamAttributes));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, null, ParameterSource.Route));
    }

    [Fact]
    public void Should_extract_route_parameter_with_description_from_constructor()
    {
        var route = "/api/{OptionalValue}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommandWithParamAttributes));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new ParameterInfo(
            "OptionalValue", typeof(string), "Optional value", false, null, ParameterSource.Route));
    }

    [Fact]
    public void Should_extract_multiple_route_parameters_with_descriptions()
    {
        var route = "/api/{Id:guid}/{Count:int}/{OptionalValue?}";
        var result = ParameterHelper.ExtractRouteParameters(route, typeof(TestCommandWithParamAttributes));

        result.Should().HaveCount(3);
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid", ParameterSource.Route));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "OptionalValue", typeof(string), "Optional value", true, null, ParameterSource.Route));
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Count", typeof(int), "", false, "int", ParameterSource.Route));
    }

    public record CommandWithBothAttributes(
        [property: Description("Property description")] Guid Id
    );

    [Fact]
    public void Should_prioritize_property_description_over_constructor_description()
    {
        var result = ParameterHelper.ExtractRouteParameters("/api/{Id}", typeof(CommandWithBothAttributes));

        result.Should().HaveCount(1);
        result.First()
              .Should().BeEquivalentTo(new ParameterInfo(
                  "Id", typeof(Guid), "Property description", false, null, ParameterSource.Route));
    }

    public record NullableTestCommand(
        Guid Id,
        string? NullableString,
        int? NullableInt,
        DateTime? NullableDateTime
    );

    [Fact]
    public void Should_correctly_identify_nullable_types_in_query_parameters()
    {
        var result = ParameterHelper.ExtractQueryParameters(typeof(NullableTestCommand));

        result.Should().HaveCount(4);

        // Non-nullable value type should not be optional
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "Id", typeof(Guid), "", false, null, ParameterSource.Query));

        // Nullable reference type should be optional
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "NullableString", typeof(string), "", true, null, ParameterSource.Query));

        // Nullable value type should be optional
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "NullableInt", typeof(int?), "", true, null, ParameterSource.Query));

        // Nullable value type should be optional
        result.Should().ContainEquivalentOf(new ParameterInfo(
            "NullableDateTime", typeof(DateTime?), "", true, null, ParameterSource.Query));
    }
}
