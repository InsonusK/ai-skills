using App.Host.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppLogging(builder.Configuration);
builder.Services.AddModules();
builder.Services.AddPipeline();

using var host = builder.Build();

// plateau-core has no API surface; demonstrate the pipeline by dispatching a command + query.
var sender = host.Services.GetRequiredService<MediatR.ISender>();
var greet = await sender.Send(new Sample.Interfaces.Commands.GreetCommand(new("world")));
Console.WriteLine($"command -> {greet.Status}: {(greet.IsSuccess ? greet.Value.Rendered : string.Join(",", greet.Errors))}");

var invalid = await sender.Send(new Sample.Interfaces.Commands.GreetCommand(new("")));
Console.WriteLine($"invalid command -> {invalid.Status}");

var last = await sender.Send(new Sample.Interfaces.Queries.GetLastGreetingQuery());
Console.WriteLine($"query -> {last.Status}: {(last.IsSuccess ? last.Value.Rendered : "none")}");
