using System.Net.Http.Json;
using CQRS.AspNet.Example;
using CQRS.AspNet.Testing;
using FluentAssertions;
using Moq;

namespace CQRS.AspNet.Tests;

public class MappingTests
{
    [Fact]
    public async Task ShouldHandleGetWithQueryParameters()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/sample-query?name=John");
        var r = await response.Content.ReadAsStringAsync();
        var content = await response.Content.ReadFromJsonAsync<SampleQueryResult>();

        content!.Address.Should().Be("123 Main St.");
    }

    [Fact]
    public async Task ShouldHandleGetWithRouteParameters()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/sample-query/John/30");
        var content = await response.Content.ReadFromJsonAsync<SampleQueryResult>();

        content!.Address.Should().Be("123 Main St.");
    }


    [Fact]
    public async Task ShouldHandleGetWithRouteAndQueryParameters()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/sample-query/John?age=30");
        var content = await response.Content.ReadFromJsonAsync<SampleQueryResult>();

        content!.Address.Should().Be("123 Main St.");
    }


    [Fact]
    public async Task ShouldHandlePostWithBody()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SampleCommand>();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/sample-command", new SampleCommand(1, "John", "123 Main St.", 30));

        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandlePostWithRouteAndBody()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SampleCommand>();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/sample-command/1", new SampleCommand(-1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandleDeleteWithRouteParameters()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<DeleteCommand>();
        var client = factory.CreateClient();
        await client.DeleteAsync("/delete-command/1");
        commandHandlerMock.VerifyCommandHandler(c => c.id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandleDeleteWithQueryParameters()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<DeleteCommand>();
        var client = factory.CreateClient();
        await client.DeleteAsync("/delete-command?id=1");
        commandHandlerMock.VerifyCommandHandler(c => c.id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandlePatchWithBody()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SampleCommand>();
        var client = factory.CreateClient();
        await client.PatchAsJsonAsync("/sample-command", new SampleCommand(1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandlePatchWithRouteAndBody()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SampleCommand>();
        var client = factory.CreateClient();
        await client.PatchAsJsonAsync("/sample-command/1", new SampleCommand(-1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.id == 1, Times.Once());
    }


    [Fact]
    public async Task ShouldHandlePutWithBody()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SampleCommand>();
        var client = factory.CreateClient();
        await client.PutAsJsonAsync("/sample-command", new SampleCommand(1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandlePutWithRouteAndBody()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SampleCommand>();
        var client = factory.CreateClient();
        await client.PutAsJsonAsync("/sample-command/1", new SampleCommand(-1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.id == 1, Times.Once());
    }
}