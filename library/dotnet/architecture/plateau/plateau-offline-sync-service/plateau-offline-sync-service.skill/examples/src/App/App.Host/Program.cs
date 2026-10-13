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

// Pipeline per Send: exception -> validation -> concurrency -> guid-resolving -> unit-of-work -> handler.
await using var scope = host.Services.CreateAsyncScope();
var sender = scope.ServiceProvider.GetRequiredService<ISender>();
var now = DateTimeOffset.UtcNow;
var clientGuid = Guid.NewGuid();   // an offline client generated this id

var added = await sender.Send(new AddItemCommand(new SoftItemTitle("buy milk"), clientGuid, now));
Console.WriteLine($"add            -> {added.Status} (id {added.Value.Id})");
var id = added.Value.Id;

// The offline client re-sends the same create (flaky network). VP6 makes it idempotent.
var replay = await sender.Send(new AddItemCommand(new SoftItemTitle("buy milk"), clientGuid, now));
Console.WriteLine($"replay (dup)   -> {replay.Status} (id {replay.Value.Id})");   // Conflict, same id

var got = await sender.Send(new GetItemQuery(id));
var renamed = await sender.Send(new RenameItemCommand(id, new SoftItemTitle("buy oat milk"), now, got.Value.Version));
Console.WriteLine($"rename         -> {renamed.Status}");

var stale = await sender.Send(new RenameItemCommand(id, new SoftItemTitle("stale"), now, got.Value.Version));
Console.WriteLine($"stale rename   -> {stale.Status}");   // Conflict — version moved

var after = await sender.Send(new GetItemQuery(id));
var done = await sender.Send(new CompleteItemCommand(id, now, after.Value.Version));
Console.WriteLine($"complete       -> {done.Status}");

var badTitle = await sender.Send(new AddItemCommand(new SoftItemTitle(""), Guid.NewGuid(), now));
Console.WriteLine($"invalid title  -> {badTitle.Status}");   // Invalid — centralized ItemTitleRules
