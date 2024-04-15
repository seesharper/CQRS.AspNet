using CQRS.AspNet;
using CQRS.AspNet.Example;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLightInject(sr => sr.RegisterFrom<CompositionRoot>());
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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



app.MapGet<SampleQuery>("/sample-query");

app.MapGet<SampleQuery>("/sample-query/{name}/{age}");
app.MapGet<SampleQuery>("/sample-query/{name}");

app.MapPost<SampleCommand>("/sample-command");
app.MapPost<SampleCommand>("/sample-command/{id}");

app.MapDelete<DeleteCommand>("/delete-command/{id}");
app.MapDelete<DeleteCommand>("/delete-command");

app.MapPatch<SampleCommand>("/sample-command");
app.MapPatch<SampleCommand>("/sample-command/{id}");

app.MapPut<SampleCommand>("/sample-command");
app.MapPut<SampleCommand>("/sample-command/{id}");





app.Run();


public partial class Program { }



