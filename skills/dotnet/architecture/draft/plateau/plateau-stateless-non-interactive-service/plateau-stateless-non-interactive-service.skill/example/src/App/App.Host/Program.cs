using App.Host.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Application.DependencyInjection;
using Sample.Interfaces.Commands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterSampleModule).Assembly);
});

builder.Services.AddModules(builder.Configuration);
builder.Services.AddPipeline();

var host = builder.Build();

var mediator = host.Services.GetRequiredService<IMediator>();

var result = await mediator.Send(new GreetCommand("World"));
Console.WriteLine(result.Value);

await host.RunAsync();
