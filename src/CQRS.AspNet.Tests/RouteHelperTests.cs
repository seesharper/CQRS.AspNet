namespace CQRS.AspNet.Tests;

using System;
using System.ComponentModel;
using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

public class RouteHelperTests
{
    public record TestCommand(
        [property: Description("The unique identifier")] Guid Id,
        [property: Description("Optional value")] string? OptionalValue,
        int Count,
        bool IsActive
    );

    [Fact]
    public void Should_extract_single_parameter_without_constraint()
    {
        var route = "/api/{Id}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new RouteParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, null));
    }

    [Fact]
    public void Should_extract_parameter_with_constraint()
    {
        var route = "/api/{Id:guid}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new RouteParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid"));
    }

    [Fact]
    public void Should_extract_optional_parameter()
    {
        var route = "/api/{OptionalValue?}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new RouteParameterInfo(
            "OptionalValue", typeof(string), "Optional value", true, null));
    }

    [Fact]
    public void Should_extract_optional_parameter_with_constraint()
    {
        var route = "/api/{OptionalValue:int?}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new RouteParameterInfo(
            "OptionalValue", typeof(string), "Optional value", true, "int"));
    }

    [Fact]
    public void Should_extract_multiple_parameters()
    {
        var route = "/api/{Id:guid}/{Count}/{IsActive}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().HaveCount(3);

        result.Should().ContainEquivalentOf(new RouteParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid"));

        result.Should().ContainEquivalentOf(new RouteParameterInfo(
            "Count", typeof(int), string.Empty, false, null));

        result.Should().ContainEquivalentOf(new RouteParameterInfo(
            "IsActive", typeof(bool), string.Empty, false, null));
    }

    [Fact]
    public void Should_throw_if_parameter_does_not_match_property()
    {
        var route = "/api/{NonExistent}";

        Action act = () => RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Route parameter 'NonExistent' does not match any property in type 'TestCommand'.");
    }

    [Fact]
    public void Should_return_empty_list_for_route_with_no_parameters()
    {
        var route = "/api/static/route";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommand));

        result.Should().BeEmpty();
    }

    public record TestCommandWithParamAttributes(
        [Description("The unique identifier")] Guid Id,
        [Description("Optional value")] string? OptionalValue,
        int Count,
        bool IsActive
    );

    [Fact]
    public void Should_extract_description_from_constructor_parameter()
    {
        var route = "/api/{Id}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommandWithParamAttributes));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new RouteParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, null));
    }

    [Fact]
    public void Should_extract_optional_parameter_from_constructor_parameter_with_description()
    {
        var route = "/api/{OptionalValue?}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommandWithParamAttributes));

        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new RouteParameterInfo(
            "OptionalValue", typeof(string), "Optional value", true, null));
    }

    [Fact]
    public void Should_extract_multiple_parameters_from_constructor_attributes()
    {
        var route = "/api/{Id:guid}/{Count}/{IsActive}";
        var result = RouteHelper.ExtractRouteParameters(route, typeof(TestCommandWithParamAttributes));

        result.Should().HaveCount(3);

        result.Should().ContainEquivalentOf(new RouteParameterInfo(
            "Id", typeof(Guid), "The unique identifier", false, "guid"));

        result.Should().ContainEquivalentOf(new RouteParameterInfo(
            "Count", typeof(int), string.Empty, false, null));

        result.Should().ContainEquivalentOf(new RouteParameterInfo(
            "IsActive", typeof(bool), string.Empty, false, null));
    }

    [Fact]
    public void Should_prefer_property_attribute_over_constructor_attribute_if_both_exist()
    {
        // This record has both attributes defined
        var result = RouteHelper.ExtractRouteParameters("/api/{Id}", typeof(CommandWithBothAttributes));

        result.Should().ContainSingle()
              .Which.Should().BeEquivalentTo(new RouteParameterInfo(
                  "Id", typeof(Guid), "From property", false, null));
    }

    public record CommandWithBothAttributes(
        [property: Description("From property")]
        [Description("From constructor")]
        Guid Id
    );
}
