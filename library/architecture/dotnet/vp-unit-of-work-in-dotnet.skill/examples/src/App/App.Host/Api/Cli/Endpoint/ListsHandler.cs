using System.Text.Json.Serialization;
using App.Host.Api.Cli.Pipeline;
using MediatR;
using Todo.Commands;

namespace App.Host.Api.Cli.Endpoint;

/// <summary>The lists resource: persistent items, reached only through commands.</summary>
public sealed class ListsHandler(ISender sender)
{
    public Resource Resource() => new("lists", "Keep to-do items in lists",
    [
        CliCommand.Command<AddItemCommand, AddItemResult>(
            "add", "Add an item to a list",
            flags: [new CliFlag("list", "the list id", Required: true), new CliFlag("title", "the item title", Required: true)],
            read: flags => new AddItemCommand(flags.Value("list")!, flags.Value("title")!),
            handle: (command, stopping) => sender.Send(command, stopping),
            encode: result => new AddedItemOutput(new ItemOutput(result.Item.Title, result.Item.Done)),
            text: result => $"[ ] {result.Item.Title}"),
        CliCommand.Command<ListItemsCommand, ListItemsResult>(
            "show", "Read a list",
            flags: [new CliFlag("list", "the list id", Required: true)],
            read: flags => new ListItemsCommand(flags.Value("list")!),
            handle: (command, stopping) => sender.Send(command, stopping),
            encode: result => new PlanItemsOutput(result.Items.Select(item => new ItemOutput(item.Title, item.Done)).ToList()),
            text: result => string.Join('\n', result.Items.Select(item => $"{(item.Done ? "[x]" : "[ ]")} {item.Title}"))),
    ]);
}

public sealed record AddedItemOutput([property: JsonPropertyName("item")] ItemOutput Item);
