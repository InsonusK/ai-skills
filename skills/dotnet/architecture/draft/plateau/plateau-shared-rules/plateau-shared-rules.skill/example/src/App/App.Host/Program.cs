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
    new SoftTitle("Review plateau example"),
    1,
    new SoftEmail("reviewer@example.com"),
    new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
    new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero)));
Console.WriteLine(result.Status);

await host.RunAsync();
