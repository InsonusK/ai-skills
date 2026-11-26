// Source: skill vp-cli-api-in-dotnet

using System.Diagnostics;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using {service-namespace}.BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using {service-namespace}.Shared.Errors;

namespace {service-namespace}.App.Host.Api.Cli.Pipeline;

/// <summary>One resource of the command line - `todo items ...` - with its actions, built by <see cref="CliCommand.Command{TCommand, TResult}"/> in the resource's file of Endpoint.</summary>
public sealed record Resource(string Name, string Short, IReadOnlyList<CliAction> Actions);

/// <summary>
/// The fixed part of the command-line entry point, copied from templates and
/// never edited: `{app} {resource} {action} [flags]` runs one command and
/// ends. RunAsync parses the arguments, runs the action they name, and
/// returns its error: an error of the action's handler as it is, so the
/// process exit code follows its kind; an error of the command line itself -
/// an unknown action, a bad flag - as invalid (exit code 2); a fault of the
/// action's glue as a failure with no kind ("Error: internal error", exit
/// code 1, the fault logged). Help goes to stdout, every error to stderr,
/// printed once as "Error: ...". Two records are logged per run
/// (<see cref="RunLog"/>), and the run gets a request id of its own. Like
/// every entry point it knows only the command layer - never domain, service,
/// or infrastructure.
/// </summary>
public static class Cli
{
    /// <summary>The name of the output-format flag of every action.</summary>
    public const string OutputFlag = "output";

    /// <summary>The output formats <see cref="OutputFlag"/> takes.</summary>
    public const string OutputJson = "json";
    public const string OutputText = "text";

    /// <summary>
    /// The CLI entry point's one call: runs the action <paramref name="args"/>
    /// names over <paramref name="resources"/>, and returns its error, or null.
    /// Tests call it through Commands.Run; the composition root lists the entry
    /// Commands.AddCliEntry builds.
    /// </summary>
    public static async Task<Exception?> RunAsync(
        ILogger logger, string name, string version, IReadOnlyList<string> args,
        IReadOnlyList<Resource> resources, TextWriter stdout, TextWriter stderr, CancellationToken stopping)
    {
        using var request = RequestId.Begin("");
        var started = Stopwatch.GetTimestamp();
        var printed = new TeeWriter(stdout);
        var actionPath = name;

        Exception? error;
        try
        {
            error = await DispatchAsync();
        }
        catch (Exception crash) // a fault of the action's glue: answered as a failure with no kind
        {
            RunLog.Crashed(logger, crash);
            error = new Exception("internal error");
        }
        RunLog.Answered(logger, actionPath, Stopwatch.GetElapsedTime(started), error is not null, printed.Captured, error is null ? null : args);
        if (error is not null)
        {
            stderr.WriteLine($"Error: {(ErrorKindException.ErrorOf(error)?.Message ?? error.Message)}");
        }
        return error;

        async Task<Exception?> DispatchAsync()
        {
            // System.CommandLine owns tokenization, command selection and option
            // parsing. We render help and errors ourselves to keep the catalog's
            // output, logging and exit-code contract; no parser action is invoked.
            var root = new RootCommand("the command line of the application");
            root.SetAction(_ => 0); // a group without a subcommand shows our help
            root.Options.Clear();
            root.Directives.Clear();
            var help = new Option<bool>("--help", "-h") { Recursive = true };
            var showVersion = new Option<bool>("--version");
            root.Options.Add(help);
            root.Options.Add(showVersion);
            var groups = new Dictionary<Command, Resource>();
            var actions = new Dictionary<Command, ActionBinding>();
            foreach (var resource in resources)
            {
                var group = new Command(resource.Name, resource.Short);
                group.SetAction(_ => 0);
                root.Subcommands.Add(group);
                groups.Add(group, resource);
                foreach (var action in resource.Actions)
                {
                    var command = new Command(action.Use, action.Short);
                    group.Subcommands.Add(command);
                    var options = new Dictionary<CliFlag, Option<string[]>>();
                    foreach (var flag in action.Flags)
                    {
                        var option = ValuesOption(flag.Name, flag.Description);
                        option.Required = flag.Required;
                        command.Options.Add(option);
                        options.Add(flag, option);
                    }
                    var output = ValuesOption(OutputFlag, "output format: json or text");
                    command.Options.Add(output);
                    actions.Add(command, new ActionBinding(resource, action, options, output));
                }
            }
            // The library drops an empty value attached by '='. Supplying it
            // as its own argument preserves an explicit empty string.
            var input = args.SelectMany(arg => arg.StartsWith("--", StringComparison.Ordinal) && arg.IndexOf('=') == arg.Length - 1
                ? new[] { arg[..^1], "" } : new[] { arg }).ToArray();
            var parsed = root.Parse(input, new ParserConfiguration { ResponseFileTokenReplacer = null });
            var selected = parsed.CommandResult.Command;
            actions.TryGetValue(selected, out var binding);
            groups.TryGetValue(selected, out var selectedGroup);
            actionPath = binding is not null ? $"{name} {binding.Resource.Name} {binding.Action.Use}"
                : selectedGroup is not null ? $"{name} {selectedGroup.Name}" : name;

            if ((parsed.Errors.Count == 0 && parsed.GetValue(help)) || args.Any(arg => arg is "--help" or "-h") || args.Count == 0 ||
                (selectedGroup is not null && parsed.Errors.Count == 0))
            {
                if (binding is not null) ActionHelp(printed, actionPath, binding.Action);
                else if (selectedGroup is not null) ResourceHelp(printed, name, selectedGroup);
                else RootHelp(printed, name, resources);
                return null;
            }
            // Unlike our CLI contract, the library accepts an unknown --flag
            // as a string option's value. Validate raw option names without
            // consuming tokens; --text=--value still carries an explicit value.
            var declared = binding?.Options.Keys.Select(flag => "--" + flag.Name).ToHashSet() ?? [];
            declared.Add("--help");
            if (binding is not null) declared.Add("--" + OutputFlag);
            if (selected == root) declared.Add("--version");
            var unknown = args.FirstOrDefault(arg => arg.StartsWith("--", StringComparison.Ordinal) && !declared.Contains(arg.Split('=', 2)[0]));
            if (unknown is not null) return Usage($"unknown flag: {unknown} (see {actionPath} --help)");

            // Array arity is checked across repeated occurrences by the
            // library. Our flags require one value for each occurrence.
            if (binding is not null)
            {
                var valueOptions = binding.Options.Values.Select(option => option.Name).Append(binding.Output.Name).ToHashSet();
                var tokens = parsed.Tokens;
                for (var i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].Type == TokenType.Option && valueOptions.Contains(tokens[i].Value) &&
                        (i + 1 == tokens.Count || tokens[i + 1].Type != TokenType.Argument))
                    {
                        return Usage($"{tokens[i].Value} wants a value (see {actionPath} --help)");
                    }
                }
            }

            if (parsed.Errors.Count > 0)
            {
                var messages = parsed.Errors.Select(error => error.Message).ToList();
                // Keep the shared usage vocabulary while retaining the parser's
                // detailed diagnostics for unknown tokens and missing values.
                if (binding is not null)
                {
                    messages.AddRange(binding.Options.Where(pair => pair.Key.Required && parsed.GetResult(pair.Value) is not { Implicit: false })
                        .Select(pair => $"--{pair.Key.Name} is required"));
                }
                return Usage(string.Join(" ", messages) + $" (see {actionPath} --help)");
            }
            if (parsed.GetValue(showVersion))
            {
                printed.WriteLine($"{name} version " + version);
                return null;
            }
            if (binding is null) return Usage($"choose an action (see {actionPath} --help)");

            var flags = new CliFlags();
            foreach (var (flag, option) in binding.Options)
            {
                foreach (var value in parsed.GetValue(option) ?? []) flags.Give(flag.Name, value);
            }
            var outputFormat = (parsed.GetValue(binding.Output) ?? []).LastOrDefault() ?? OutputJson;
            if (outputFormat is not (OutputJson or OutputText))
            {
                return Usage($"--output must be json or text, not \"{outputFormat}\"");
            }
            RunLog.Received(logger, actionPath, flags.Given());
            return await binding.Action.RunAsync(flags, outputFormat, printed, stopping);
        }

        static ErrorKindException Usage(string message) => new(ErrorKind.Invalid, message);
    }

    // One value per occurrence, including repeated flags. Arrays retain their
    // order; CliFlags.Value returns the last value of a scalar flag.
    private static Option<string[]> ValuesOption(string name, string description) => new("--" + name)
    {
        Description = description,
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = false,
    };

    private sealed record ActionBinding(Resource Resource, CliAction Action,
        Dictionary<CliFlag, Option<string[]>> Options, Option<string[]> Output);

    private static void RootHelp(TextWriter output, string name, IReadOnlyList<Resource> resources)
    {
        output.WriteLine($"{name} - the command line of the application");
        output.WriteLine();
        output.WriteLine("Usage:");
        output.WriteLine($"  {name} [command]");
        output.WriteLine();
        output.WriteLine("Available Commands:");
        foreach (var resource in resources)
        {
            output.WriteLine($"  {resource.Name,-20}{resource.Short}");
        }
        output.WriteLine();
        output.WriteLine("Flags:");
        output.WriteLine($"  -h, --help        help for {name}");
        output.WriteLine("      --version     the version of the binary");
        output.WriteLine();
        output.WriteLine($"Use \"{name} [command] --help\" for more information about a command.");
    }

    private static void ResourceHelp(TextWriter output, string name, Resource resource)
    {
        output.WriteLine(resource.Short);
        output.WriteLine();
        output.WriteLine("Usage:");
        output.WriteLine($"  {name} {resource.Name} [action]");
        output.WriteLine();
        output.WriteLine("Available Commands:");
        foreach (var action in resource.Actions)
        {
            output.WriteLine($"  {action.Use,-20}{action.Short}");
        }
    }

    private static void ActionHelp(TextWriter output, string actionPath, CliAction action)
    {
        output.WriteLine(action.Short);
        output.WriteLine();
        output.WriteLine("Usage:");
        output.WriteLine($"  {actionPath} [flags]");
        output.WriteLine();
        output.WriteLine("Flags:");
        foreach (var flag in action.Flags)
        {
            var required = flag.Required ? ", required" : "";
            output.WriteLine($"      --{flag.Name,-16}{flag.Description}{required}");
        }
        output.WriteLine($"      --{OutputFlag,-16}output format: json or text (default \"json\")");
        output.WriteLine("  -h, --help           help for this action");
    }
}

/// <summary>Writes through and keeps a copy, so the record of the run can hold the output as printed.</summary>
internal sealed class TeeWriter(TextWriter inner) : TextWriter
{
    private readonly StringWriter _captured = new();

    public string Captured => _captured.ToString().TrimEnd('\r', '\n');

    public override Encoding Encoding => inner.Encoding;

    public override void Write(char value)
    {
        inner.Write(value);
        _captured.Write(value);
    }
}
