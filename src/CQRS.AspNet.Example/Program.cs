using CQRS.AspNet;
using CQRS.AspNet.Example;
using CQRS.Query.Abstractions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLightInject(sr => sr.RegisterFrom<CompositionRoot>());
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("RestfulClient", client =>
{
    client.BaseAddress = new Uri("https://api.restful-api.dev/");
}).AddAsKeyed();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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

app.MapGet<SampleQuery>("/sample-query/{name}/{age}");
app.MapGet<SampleQuery>("/sample-query/{name}");

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



