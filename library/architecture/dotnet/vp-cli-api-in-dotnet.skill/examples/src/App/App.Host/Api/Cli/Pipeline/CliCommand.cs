// Source: skill vp-cli-api-in-dotnet

using System.Text.Json;

namespace App.Host.Api.Cli.Pipeline;

/// <summary>One flag of an action, declared by its bind.</summary>
/// <param name="Name">The flag as typed, without the dashes: "title".</param>
/// <param name="Description">One line of help.</param>
/// <param name="Required">A run without the flag is a usage error ("--title is required").</param>
/// <param name="Many">The flag may repeat; its values keep their order. A repeated value is a repeated flag, not a comma-separated string.</param>
public sealed record CliFlag(string Name, string Description, bool Required = false, bool Many = false);

/// <summary>The flags one run gave, by name.</summary>
public sealed class CliFlags
{
    private readonly Dictionary<string, List<string>> _values = [];

    /// <summary>True when the run gave the flag.</summary>
    public bool Has(string name) => _values.ContainsKey(name);

    /// <summary>The flag's value; the last one when it repeats, null when absent.</summary>
    public string? Value(string name) => _values.TryGetValue(name, out var values) ? values[^1] : null;

    /// <summary>Every value the flag was given, in order.</summary>
    public IReadOnlyList<string> Values(string name) => _values.TryGetValue(name, out var values) ? values : [];

    /// <summary>The flags as given, for the record of the run: each name, its values joined by a comma.</summary>
    public IReadOnlyDictionary<string, string> Given() => _values.ToDictionary(pair => pair.Key, pair => string.Join(",", pair.Value));

    internal void Give(string name, string value)
    {
        if (!_values.TryGetValue(name, out var values))
        {
            _values[name] = values = [];
        }
        values.Add(value);
    }
}

/// <summary>One action of a resource; build it with <see cref="CliCommand.Command{TCommand, TResult}"/>.</summary>
public abstract record CliAction(string Use, string Short)
{
    /// <summary>The action's flags, without --output, which every action has.</summary>
    public abstract IReadOnlyList<CliFlag> Flags { get; }

    /// <summary>Runs the action: reads the command, calls the handler, prints the result. A failure of the handler is returned with its kind kept; an error of read is a <see cref="FormatException"/> (a usage error); anything else that escapes is a crash of the action.</summary>
    public abstract Task<Exception?> RunAsync(CliFlags flags, string output, TextWriter stdout, CancellationToken stopping);
}

/// <summary>
/// Turns one command handler into one action of the command line, written once
/// per command: <paramref name="flags"/> declares the action's flags;
/// <paramref name="read"/> turns the parsed flags into the command (a
/// <see cref="FormatException"/> of it is a usage error, exit code 2, and the
/// handler is not called); the handler runs the command; a failure of it is
/// returned with its kind kept; <paramref name="encode"/> turns the result
/// into the output written as one JSON line, or, with --output text,
/// <paramref name="text"/> writes it for a person - when text is null,
/// --output text writes indented JSON. Flags, encode, and text belong to the
/// action (its resource's file of Endpoint): the flags and the output are the
/// command line's contract, never the command's types.
/// </summary>
public static class CliCommand
{
    public static CliAction Command<TCommand, TResult>(
        string use, string shortHelp,
        IReadOnlyList<CliFlag> flags,
        Func<CliFlags, TCommand> read,
        Func<TCommand, CancellationToken, Task<TResult>> handle,
        Func<TResult, object> encode,
        Func<TResult, string?>? text = null)
        where TCommand : notnull =>
        new CliAction<TCommand, TResult>(use, shortHelp, flags, read, handle, encode, text);

    private sealed record CliAction<TCommand, TResult>(
        string Use,
        string Short,
        IReadOnlyList<CliFlag> ActionFlags,
        Func<CliFlags, TCommand> Read,
        Func<TCommand, CancellationToken, Task<TResult>> Handle,
        Func<TResult, object> Encode,
        Func<TResult, string?>? Text) : CliAction(Use, Short)
    {
        public override IReadOnlyList<CliFlag> Flags => ActionFlags;

        public override async Task<Exception?> RunAsync(CliFlags flags, string output, TextWriter stdout, CancellationToken stopping)
        {
            var command = Read(flags); // a FormatException of read is a usage error of Cli.RunAsync
            TResult result;
            try
            {
                result = await Handle(command, stopping);
            }
            catch (Exception failure)
            {
                return failure; // the handler's error keeps its kind and its message
            }
            Print(stdout, output, Encode(result), Text?.Invoke(result)); // a fault here is a crash of the action
            return null;
        }

        private static void Print(TextWriter stdout, string output, object encoded, string? text)
        {
            if (output == Cli.OutputText && text is not null)
            {
                stdout.WriteLine(text);
                return;
            }
            var options = output == Cli.OutputText ? new JsonSerializerOptions { WriteIndented = true } : null;
            stdout.WriteLine(JsonSerializer.Serialize(encoded, options));
        }
    }
}
