using System.Net;
using System.Net.Http.Json;
using CQRS.AspNet.Example;
using CQRS.AspNet.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CQRS.AspNet.Tests;

public class MappingTests
{
    [Fact]
    public async Task ShouldHandleGetWithQueryParameters()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/sample-query?name=John&Age=30");
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
    public void ShouldThrowExceptionWhenCommandIsUsedInGetEndpoint()
    {
        var factory = new TestApplication<Program>();
        factory.WithConfiguration("MapGetEndpointWithCommand", "true");
        Action act = () => factory.CreateClient();
        act.Should().Throw<InvalidOperationException>().WithMessage("Type SampleCommand is not a query. Only queries can be used in get endpoints");
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
        commandHandlerMock.VerifyCommandHandler(c => c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandleDeleteWithQueryParameters()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<DeleteCommand>();
        var client = factory.CreateClient();
        await client.DeleteAsync("/delete-command?id=1");
        commandHandlerMock.VerifyCommandHandler(c => c.Id == 1, Times.Once());
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

    [Fact]
    public async Task ShouldHandlePostWithAttribute()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<CommandWithAttribute>();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/command-with-attribute", new CommandWithAttribute(1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandlePutWithAttribute()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<CommandWithAttribute>();
        var client = factory.CreateClient();
        await client.PutAsJsonAsync("/command-with-attribute/1", new CommandWithAttribute(-1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandlePatchWithAttribute()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<CommandWithAttribute>();
        var client = factory.CreateClient();
        await client.PatchAsJsonAsync("/command-with-attribute/1", new CommandWithAttribute(-1, "John", "123 Main St.", 30));
        commandHandlerMock.VerifyCommandHandler(c => c.Address == "123 Main St." && c.Age == 30 && c.Name == "John" && c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandleDeleteWithAttribute()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<DeleteCommandWithAttribute>();
        var client = factory.CreateClient();
        await client.DeleteAsync("/delete-command-with-attribute/1");
        commandHandlerMock.VerifyCommandHandler(c => c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandleGetWithAttribute()
    {
        var factory = new TestApplication<Program>();
        var queryHandlerMock = factory.MockQueryHandler<QueryWithAttribute, QueryWithAttributeResult>();
        queryHandlerMock.Setup(c => c.HandleAsync(It.IsAny<QueryWithAttribute>(), It.IsAny<CancellationToken>())).ReturnsAsync(new QueryWithAttributeResult("John", "123 Main St."));
        var client = factory.CreateClient();
        await client.GetAsync("/query-with-attribute/1");
        queryHandlerMock.VerifyQueryHandler(c => c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldMapCqrsEndpointsFromGivenAssembly()
    {
        var factory = new TestApplication<Program>();
        var queryHandlerMock = factory.MockQueryHandler<QueryWithAttribute, QueryWithAttributeResult>();
        factory.WithConfiguration("UseCqrsEndpointsFromAssembly", "true");
        var client = factory.CreateClient();
        await client.GetAsync("/query-with-attribute/1");
        queryHandlerMock.VerifyQueryHandler(c => c.Id == 1, Times.Once());
    }

    [Fact]
    public async Task ShouldHandleQueryWithTypedResults()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/query-with-typed-results/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response = await client.GetAsync("/query-with-typed-results/2");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShouldPostWithTypedResults()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/post-command-with-result", new PostCommandWithResult(1));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ShouldPatchWithTypedResults()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var response = await client.PatchAsJsonAsync("/patch-command-with-result", new PatchCommandWithResult(1));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShouldPutWithTypedResults()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var response = await client.PutAsJsonAsync("/put-command-with-result", new PutCommandWithResult(1));
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShouldDeleteWithTypedResults()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var result = await client.DeleteFromJsonAsync<DeleteCommandResult>("/delete-command-with-result/1");
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task ShouldThrowExceptionWhenCommandResultIsNotSet()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/command-without-setting-result", new CommandWithoutSettingResult(1));
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ShouldUseResultWhenInheritingFromCommand()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/command-inheriting-from-create-command", new CommandInheritingFromCreateCommand());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}