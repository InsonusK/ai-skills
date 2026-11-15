using System.Text.Json.Serialization;
using App.Host.Api.Cli.Pipeline;
using MediatR;
using Todo.Commands;

namespace App.Host.Api.Cli.Endpoint;

// This file is the items resource (`todo items ...`): its actions, and per
// action its flags, its output type, and its text form. A new command on
// items adds an action line and its bind, encode, and text functions here.

/// <summary>The items resource: `todo items`, with its actions. It sends its commands through <see cref="ISender"/>, so every pipeline behavior runs.</summary>
public sealed class ItemsHandler(ISender sender)
{
    /// <summary>`todo items` with its actions.</summary>
    public Resource Resource() => new("items", "Plan to-do items",
    [
        CliCommand.Command<PlanItemsCommand, PlanItemsResult>(
            "plan", "Plan one open item per --title",
            flags: [new CliFlag("title", "the title of an item; repeat for several", Many: true)],
            read: flags => new PlanItemsCommand(flags.Values("title")),
            handle: (command, stopping) => sender.Send(command, stopping),
            encode: result => new PlanItemsOutput(result.Items.Select(item => new ItemOutput(item.Title, item.Done)).ToList()),
            text: result => string.Join('\n', result.Items.Select(item => $"{(item.Done ? "[x]" : "[ ]")} {item.Title}"))),
    ]);
}

/// <summary>The JSON line of `todo items plan`.</summary>
public sealed record PlanItemsOutput([property: JsonPropertyName("items")] IReadOnlyList<ItemOutput> Items);

/// <summary>One item of the output.</summary>
public sealed record ItemOutput([property: JsonPropertyName("title")] string Title, [property: JsonPropertyName("done")] bool Done);
