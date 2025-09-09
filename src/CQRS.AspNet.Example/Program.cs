using System.ComponentModel;
using CQRS.AspNet;
using CQRS.AspNet.Example;
using CQRS.Command.Abstractions;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLightInject(sr => sr.RegisterFrom<CompositionRoot>());
builder.Services.AddHttpClient("RestfulClient", client =>
{
    client.BaseAddress = new Uri("https://api.restful-api.dev/");
}).AddAsKeyed(lifetime: ServiceLifetime.Singleton);


builder.Services.AddKeyedTransient<IKeyedService, CQRS.AspNet.Example.KeyedService>("KeyedService");
builder.Services.AddKeyedTransient<IKeyedService, CQRS.AspNet.Example.AnotherKeyedService>("AnotherKeyedService");
builder.Services.AddOpenApi("v1");

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Configuration.GetValue<bool>("UseCqrsEndpointsFromAssembly"))
{
    app.MapCqrsEndpoints(typeof(Program).Assembly);
}
else
{
    app.MapCqrsEndpoints();
}

if (app.Configuration.GetValue<bool>("MapGetEndpointWithCommand"))
{
    app.MapGet<SampleCommand>("sample-command"); ;
}

//app.MapPost("query-as-post", (IQueryExecutor queryExecutor, QueryAsPost query) => queryExecutor.ExecuteAsync(query));


// app.MapPost("post-command-without-body/{id}", (IQueryExecutor queryExecutor, [AsParameters] PostCommandWithoutBody command) =>
// {
//     Console.WriteLine("");
// });




app.MapGet<SampleQuery>("/sample-query");


app.MapPost("whatever", async ([AsParameters] SampleParameters parameters, [FromBody] SampleQuery query) =>
{
    return TypedResults.Ok();
});

app.MapGet<SampleQuery>("/sample-query/{name}/{age}");
app.MapGet<SampleQuery>("/sample-query/{name}");

var test = app.MapPost("testing/{name}/{age}", async (HttpRequest request, ICommandExecutor commandExecutor, [AsParameters] SampleParameters parameters, [FromBody] PostCommandWithoutBody command) =>
{
    //return TypedResults.Ok(42);
});




app.MapPost<SampleCommand>("/sample-command");
app.MapPost<SampleCommand>("/sample-command/{id}");

app.MapDelete<SampleDeleteCommand>("/delete-command/{id}");
app.MapDelete<SampleDeleteCommand>("/delete-command");

app.MapPatch<SampleCommand>("/sample-command");
app.MapPatch<SampleCommand>("/sample-command/{id}");

app.MapPut<SampleCommand>("/sample-command");
app.MapPut<SampleCommand>("/sample-command/{id}");

app.Run();

public partial class Program { }



public record SampleParameters([Description("This is the name")] string Name, int Age);