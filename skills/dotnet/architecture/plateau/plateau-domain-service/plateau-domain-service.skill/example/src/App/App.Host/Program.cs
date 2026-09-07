using App.Host.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Interfaces.Commands;
using Sample.Interfaces.Queries;
using Sample.Interfaces.ValueObjects;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppLogging(builder.Configuration);
builder.Services.AddModules();
builder.Services.AddPipeline();
builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();

// Demonstrate the domain-service composition end to end. Each Send goes through the full
// pipeline: exception -> validation -> concurrency -> handler -> unit-of-work commit.
await using var scope = host.Services.CreateAsyncScope();
var sender = scope.ServiceProvider.GetRequiredService<ISender>();
var now = DateTimeOffset.UtcNow;

var added = await sender.Send(new AddItemCommand(new SoftItemTitle("buy milk"), now));
Console.WriteLine($"add       -> {added.Status} (id {added.Value.Id})");
var id = added.Value.Id;

var got = await sender.Send(new GetItemQuery(id));
Console.WriteLine($"get       -> {got.Status}: \"{got.Value.Title}\" v{got.Value.Version} done={got.Value.IsDone}");

var renamed = await sender.Send(new RenameItemCommand(id, new SoftItemTitle("buy oat milk"), now, got.Value.Version));
Console.WriteLine($"rename    -> {renamed.Status}");

var stale = await sender.Send(new RenameItemCommand(id, new SoftItemTitle("stale write"), now, got.Value.Version));
Console.WriteLine($"stale     -> {stale.Status}");   // Conflict — version moved on

var afterRename = await sender.Send(new GetItemQuery(id));
var completed = await sender.Send(new CompleteItemCommand(id, now, afterRename.Value.Version));
Console.WriteLine($"complete  -> {completed.Status}");

var renameDone = await sender.Send(new RenameItemCommand(id, new SoftItemTitle("nope"), now, afterRename.Value.Version + 1));
Console.WriteLine($"rename@done-> {renameDone.Status}");   // Error — domain guard rejects renaming a completed item

var invalid = await sender.Send(new AddItemCommand(new SoftItemTitle(""), now));
Console.WriteLine($"invalid   -> {invalid.Status}");
