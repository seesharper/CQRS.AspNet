using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CQRS.AspNet.Example;
using CQRS.AspNet.MetaData;
using CQRS.AspNet.Testing;
using CQRS.Query.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Moq;
using RichardSzalay.MockHttp;

namespace CQRS.AspNet.Tests;

public class HttpClientExtensionTests
{
    [Fact]
    public async Task ShouldHandleResponse()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond("application/json", "{name : \"Test McGee\"}"); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");
        var response = await client.SendAndHandleResponse(request);

        Assert.NotNull(response);
    }

    [Fact]
    public async Task ShouldHandleResponseWithTypedResult()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond("application/json", new User("Test McGee", 30).ToJson()); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");
        var response = await client.SendAndHandleResponse<User>(request);

        Assert.NotNull(response);
        Assert.Equal("Test McGee", response.Name);
    }


    [Fact]
    public async Task ShouldHandleResponseWithProblemDetails()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond(HttpStatusCode.BadRequest, "application/problem+json", "{\"title\" : \"This is a bad request\"}"); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAndHandleResponse(request);
        });

        // Assert that the exception contains the problem details
        Assert.Contains("This is a bad request", exception.Message);
    }


    [Fact]
    public async Task ShouldHandleProblemDetailsWithInvalidJson()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond(HttpStatusCode.BadRequest, "application/problem+json", "rubbish"); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");

        var exception = await Assert.ThrowsAsync<JsonException>(async () =>
        {
            await client.SendAndHandleResponse(request);
        });

        // Assert that the exception contains the problem details        
        Assert.Contains("rubbish", exception.Message);
    }



    [Fact]
    public async Task ShouldHandleErrorWithoutProblemDetails()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond(HttpStatusCode.BadRequest, "text/plain", "This is a bad request"); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAndHandleResponse(request);
        });
        // Assert that the exception contains the raw response
        Assert.Contains("This is a bad request", exception.Message ?? string.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

    }

    [Fact]
    public async Task ShouldHandleInvalidJsonWithAsMethod()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond(HttpStatusCode.OK, "application/json", "rubbish"); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");

        var exception = await Assert.ThrowsAsync<JsonException>(async () =>
        {
            await client.SendAndHandleResponse<User>(request);
        });



        // Assert that the exception contains the problem details
        Assert.Contains("rubbish", exception.Message);
    }

    [Fact]
    public async Task ShouldHandleNullJsonWithAsMethod()
    {
        var mockHttp = new MockHttpMessageHandler();

        // Setup a respond for the user api (including a wildcard in the URL)
        mockHttp.When("http://localhost/api/user/*")
                .Respond(HttpStatusCode.OK, request => JsonContent.Create<User?>(null)); // Respond with JSON

        // Inject the handler or client into your application code
        var client = mockHttp.ToHttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/user/1234");

        var exception = await Assert.ThrowsAsync<JsonException>(async () =>
        {
            await client.SendAndHandleResponse<User>(request);
        });



        // Assert that the exception contains the problem details
        Assert.Contains("There was a problem deserializing", exception.Message);
    }

    [Fact]
    public async Task ShouldThrowExceptionWhenPropertyIsMissingOnQuery()
    {

    }



    [Fact]
    public async Task ShouldGetQuery()
    {
        var client = CreateClient(new User("Test McGee", 20));
        var user = await client.Get(new UserQuery(1));
        Assert.NotNull(user);
        Assert.Equal("Test McGee", user.Name);
        Assert.Equal(20, user.Age);
    }

    [Fact]
    public async Task ShouldHandlePostCommand()
    {
        var factory = new TestApplication<Program>();

        var client = factory.CreateClient();
        await client.Post(new SamplePostCommand(10, "Test McGee", 20));

    }

    [Fact]
    public async Task ShouldHandlePostCommandFromBase()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var result = await client.Post(new SamplePostCommandWithResult(10));
        result.Should().Be(10);
    }






    [Fact]
    public async Task ShouldHandlePostCommandWithValueType()
    {
        var factory = new TestApplication<Program>();

        var client = factory.CreateClient();
        await client.Post(new SamplePostCommandWithValueType(10, "Test McGee", 20));
    }


    [Fact]
    public async Task ShouldHandlePostWithProblem()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.Post(new SamplePostCommandWithProblem(10));
        });
    }



    [Fact]
    public async Task ShouldHandlePostCommandWithInvalidProperty()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SamplePostCommandWithInvalidProperty>();
        var client = factory.CreateClient();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
       {
           await client.Post(new SamplePostCommandWithInvalidProperty(10, "Test McGee", 20));
       });
    }


    [Fact]
    public async Task ShouldHandleGetQueryWithInvalidProperty()
    {
        var factory = new TestApplication<Program>();
        var queryHandlerMock = factory.MockQueryHandler<SampleGetQueryWithInvalidProperty, SampleGetQueryWithInvalidPropertyResult>().Returns(new SampleGetQueryWithInvalidPropertyResult("Test McGee", "123 Main St."));
        var client = factory.CreateClient();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
       {
           await client.Get(new SampleGetQueryWithInvalidProperty("Test McGee"));
       });
    }


    [Fact]
    public async Task ShouldThrowExceptionWhenHandlingPostCommandPropertyValueIsNull()
    {
        var factory = new TestApplication<Program>();
        var commandHandlerMock = factory.MockCommandHandler<SamplePostCommand>();
        var client = factory.CreateClient();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
       {
           await client.Post(new SamplePostCommand(null, "Test McGee", 20));
       });
    }


    [Fact]
    public async Task ShouldHandlePatch()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        await client.Patch(new SamplePatchCommand(10));
    }

    [Fact]
    public async Task ShouldHandleDelete()
    {
        var factory = new TestApplication<Program>();
        var client = factory.CreateClient();
        await client.Delete(new SampleDeleteCommandFromBase(10));
    }

    [Fact]
    public async Task ShouldHandleGetQueryWithQueryParameters()
    {
        var factory = new TestApplication<Program>();
        var queryHandlerMock = factory.MockQueryHandler<SampleGetQueryWithQueryParameters, SampleGetQueryWithQueryParametersResult>().Returns(new SampleGetQueryWithQueryParametersResult("Test McGee", "123 Main St."));

        var client = factory.CreateClient();
        var result = await client.Get(new SampleGetQueryWithQueryParameters("Test McGee", 20));
        queryHandlerMock.VerifyQueryHandler(
            query => query.Name == "Test McGee" && query.Age == 20,
            Times.Once());
    }

    [Fact]
    public async Task ShouldHandleGetQueryWithRouteValues()
    {
        var factory = new TestApplication<Program>();
        var queryHandlerMock = factory.MockQueryHandler<SampleGetQueryWithRouteValues, SampleGetQueryWithRouteValuesResult>().Returns(new SampleGetQueryWithRouteValuesResult("Test McGee", "123 Main St."));

        var client = factory.CreateClient();
        var result = await client.Get(new SampleGetQueryWithRouteValues("Test McGee", 20));
        queryHandlerMock.VerifyQueryHandler(
            query => query.Name == "Test McGee" && query.Age == 20,
            Times.Once());
    }

    [Fact]
    public async Task ShouldThrowExceptionWhenHandlingGetQueryPropertyValueIsNull()
    {
        var factory = new TestApplication<Program>();
        var queryHandlerMock = factory.MockQueryHandler<SampleGetQueryWithRouteValues, SampleGetQueryWithRouteValuesResult>().Returns(new SampleGetQueryWithRouteValuesResult("Test McGee", "123 Main St."));

        var client = factory.CreateClient();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
       {
           await client.Get(new SampleGetQueryWithRouteValues(null, 20));
       });
    }




    private HttpClient CreateClient<TResponse>(TResponse content, HttpStatusCode statusCode = HttpStatusCode.OK)
        where TResponse : class
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("http://localhost/api/user/*")
                .Respond(statusCode, JsonContent.Create(content)); // Respond with JSON
        var client = mockHttp.ToHttpClient();
        client.BaseAddress = new Uri("http://localhost");
        return client;
    }
}

[Get("api/user/{Id}")]
public record UserQuery(int Id) : IQuery<User>;

public record User(string Name, int Age);


