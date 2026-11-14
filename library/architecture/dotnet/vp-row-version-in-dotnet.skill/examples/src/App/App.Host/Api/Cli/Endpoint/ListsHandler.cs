using System.Text.Json.Serialization;
using System.Globalization;
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
            encode: result => new AddedItemOutput(Output(result.Item)),
            text: result => Text(result.Item)),
        CliCommand.Command<ListItemsCommand, ListItemsResult>(
            "show", "Read a list",
            flags: [new CliFlag("list", "the list id", Required: true)],
            read: flags => new ListItemsCommand(flags.Value("list")!),
            handle: (command, stopping) => sender.Send(command, stopping),
            encode: result => new ListItemsOutput(result.Items.Select(Output).ToList()),
            text: result => string.Join('\n', result.Items.Select(Text))),
        CliCommand.Command<CompleteItemCommand, CompleteItemResult>(
            "complete", "Complete an item under the version read",
            flags: [new CliFlag("list", "the list id", Required: true), new CliFlag("id", "the item id", Required: true), new CliFlag("version", "the version read", Required: true)],
            read: flags => new CompleteItemCommand(flags.Value("list")!, ReadId(flags.Value("id")!), flags.Value("version")!),
            handle: (command, stopping) => sender.Send(command, stopping),
            encode: result => new AddedItemOutput(Output(result.Item)),
            text: result => Text(result.Item)),
    ]);

    private static long ReadId(string value) => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
        ? id : throw new FormatException("--id must be an integer");

    private static ListItemOutput Output(ItemView item) =>
        new(item.Id!.Value.ToString(CultureInfo.InvariantCulture), item.Version!, item.Title, item.Done);

    private static string Text(ItemView item) =>
        $"{(item.Done ? "[x]" : "[ ]")} {item.Title} (id: {item.Id}, version: {item.Version})";
}

public sealed record AddedItemOutput([property: JsonPropertyName("item")] ListItemOutput Item);

public sealed record ListItemsOutput([property: JsonPropertyName("items")] IReadOnlyList<ListItemOutput> Items);

public sealed record ListItemOutput(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("done")] bool Done);
