using App.Host.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Interfaces.Commands;
using Sample.Interfaces.ValueObjects;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModules(builder.Configuration);
builder.Services.AddPipeline();

var host = builder.Build();

var mediator = host.Services.GetRequiredService<IMediator>();

var result = await mediator.Send(new CreateTaskCommand(
    "Review plateau example",
    1,
    new SoftEmail("reviewer@example.com")));
Console.WriteLine(result.Status);

await host.RunAsync();
