// Source: skill vp-cli-api-in-dotnet

using System.Diagnostics;
using System.Text;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace App.Host.Api.Cli.Pipeline;

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
            if (args.Count == 0 || IsHelp(args[0]))
            {
                RootHelp(printed, name, resources);
                return null;
            }
            if (args is ["--version"])
            {
                printed.WriteLine($"{name} version " + version); // concatenation: the templates check fills braces
                return null;
            }
            var resource = resources.FirstOrDefault(known => known.Name == args[0]);
            if (resource is null)
            {
                return Usage($"unknown command \"{args[0]}\" (see {name} --help)");
            }
            var rest = args.Skip(1).ToList();
            if (rest.Count == 0 || IsHelp(rest[0]))
            {
                ResourceHelp(printed, name, resource);
                return null;
            }
            var action = resource.Actions.FirstOrDefault(known => known.Use == rest[0]);
            if (action is null)
            {
                return Usage($"unknown action \"{rest[0]}\" (see {name} {resource.Name} --help)");
            }
            actionPath = $"{name} {resource.Name} {action.Use}";
            var given = rest.Skip(1).ToList();
            if (given.Any(IsHelp))
            {
                ActionHelp(printed, actionPath, action);
                return null;
            }
            var (flags, output, parseError) = ParseFlags(given, action, actionPath);
            if (parseError is not null)
            {
                return parseError;
            }
            var missing = action.Flags.FirstOrDefault(flag => flag.Required && !flags.Has(flag.Name));
            if (missing is not null)
            {
                return Usage($"--{missing.Name} is required");
            }
            RunLog.Received(logger, actionPath, flags.Given());
            try
            {
                return await action.RunAsync(flags, output, printed, stopping);
            }
            catch (FormatException read)
            {
                return Usage($"{read.Message} (see {actionPath} --help)");
            }
        }

        static bool IsHelp(string arg) => arg is "--help" or "-h";

        static ErrorKindException Usage(string message) => new(ErrorKind.Invalid, message);
    }

    // The flags of one action: its own, and --output. An unknown flag or a
    // flag without its value is a usage error.
    private static (CliFlags Given, string Output, ErrorKindException? Error) ParseFlags(
        IReadOnlyList<string> args, CliAction action, string actionPath)
    {
        var flags = new CliFlags();
        var output = OutputJson;
        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            string flag;
            string? value;
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                var equals = arg.IndexOf('=');
                flag = equals < 0 ? arg[2..] : arg[2..equals];
                value = equals < 0 ? null : arg[(equals + 1)..];
            }
            else
            {
                return (flags, output, new ErrorKindException(ErrorKind.Invalid, $"unexpected argument \"{arg}\" (see {actionPath} --help)"));
            }
            if (flag == OutputFlag)
            {
                var (outputValue, outputError) = value is null ? Next(args, ref i, flag, actionPath) : (value, null);
                if (outputError is not null)
                {
                    return (flags, output, outputError);
                }
                if (outputValue is not (OutputJson or OutputText))
                {
                    return (flags, output, new ErrorKindException(ErrorKind.Invalid, $"--output must be json or text, not \"{outputValue}\""));
                }
                output = outputValue;
                continue;
            }
            var declared = action.Flags.FirstOrDefault(known => known.Name == flag);
            if (declared is null)
            {
                return (flags, output, new ErrorKindException(ErrorKind.Invalid, $"unknown flag: --{flag} (see {actionPath} --help)"));
            }
            var (flagValue, valueError) = value is null ? Next(args, ref i, flag, actionPath) : (value, null);
            if (valueError is not null)
            {
                return (flags, output, valueError);
            }
            flags.Give(flag, flagValue!);
        }
        return (flags, output, null);
    }

    private static (string? Value, ErrorKindException? Error) Next(IReadOnlyList<string> args, ref int i, string flag, string actionPath)
    {
        if (i + 1 < args.Count && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            return (args[++i], null);
        }
        return (null, new ErrorKindException(ErrorKind.Invalid, $"--{flag} wants a value (see {actionPath} --help)"));
    }

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
