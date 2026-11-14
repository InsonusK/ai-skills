using System.Collections.Concurrent;
using App.Host.Api.Cli;
using App.Host.Api.Cli.Pipeline;
using CliPipeline = App.Host.Api.Cli.Pipeline.Cli;
using BuildingBlocks.Logging;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Shared.Errors;
using Xunit;

namespace App.Host.Tests.Steps.Cli;

/// <summary>One record as a part wrote it: every attribute, the payload too.</summary>
public sealed record CapturedRecord(string Category, LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Attributes, Exception? Exception);

/// <summary>
/// A logger provider that keeps every record with all its attributes, so the
/// scenarios see everything the entry point writes - what the base provider
/// then drops is the BuildingBlocks scenarios' concern.
/// </summary>
public sealed class CapturedLogs : ILoggerProvider
{
    private readonly ConcurrentQueue<CapturedRecord> _records = [];

    public CapturedLogs() => Factory = LoggerFactory.Create(logging => logging.SetMinimumLevel(LogLevel.Trace).AddProvider(this));

    /// <summary>The factory to hand to the code under test.</summary>
    public ILoggerFactory Factory { get; }

    public IReadOnlyList<CapturedRecord> Records => [.. _records];

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _records);

    public void Dispose()
    {
    }

    private sealed class Logger(string category, ConcurrentQueue<CapturedRecord> records) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var attributes = (state as IEnumerable<KeyValuePair<string, object?>> ?? [])
                .Where(pair => pair.Key != LogState.OriginalFormatKey)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            records.Enqueue(new CapturedRecord(category, logLevel, formatter(state, exception), attributes, exception));
        }
    }
}

/// <summary>
/// One run of the command line: the streams, the error it returned, and the
/// records it wrote. Shared by the steps of the CLI features of the example -
/// scoped to them, as "running should end without an error" is also the entry
/// runner's step.
/// </summary>
[Binding]
[Scope(Feature = "Command line")]
[Scope(Feature = "items commands")]
[Scope(Feature = "lists commands in dotnet")]
public sealed class CliWorld
{
    private readonly StringWriter _stdout = new();
    private readonly StringWriter _stderr = new();

    public CapturedLogs Logs { get; } = new();

    public Exception? Error { get; private set; }

    public string Stdout => _stdout.ToString();

    public string Stderr => _stderr.ToString();

    public async Task RunAsync(string name, string version, string args, IReadOnlyList<Resource> resources)
    {
        Error = await CliPipeline.RunAsync(Logs.Factory.CreateLogger("test"), name, version, SplitArgs(args), resources, _stdout, _stderr, CancellationToken.None);
    }

    // The arguments as one line: the scenarios give no quoting, so a space
    // separates them; an empty line is no arguments.
    private static IReadOnlyList<string> SplitArgs(string args) =>
        args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [Then("running should end without an error")]
    public void ThenRunningShouldEndWithoutAnError() => Assert.Null(Error);

    [Then("running should fail with the kind {string}")]
    public void ThenRunningShouldFailWithTheKind(string kind) =>
        Assert.Equal(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), ErrorKindException.KindOf(Error));

    [Then("running should fail with no kind")]
    public void ThenRunningShouldFailWithNoKind()
    {
        Assert.NotNull(Error);
        Assert.Null(ErrorKindException.KindOf(Error));
    }

    [Then("stdout should be the line {string}")]
    public void ThenStdoutShouldBeTheLine(string line) => Assert.Equal(line + Environment.NewLine, Stdout);

    [Then("stdout should be the lines {string}")]
    public void ThenStdoutShouldBeTheLines(string lines) => Assert.Equal(string.Join('\n', lines.Split('|')) + Environment.NewLine, Stdout);

    [Then("stdout should mention {string}")]
    public void ThenStdoutShouldMention(string text) => Assert.Contains(text, Stdout);

    [Then("stdout should be empty")]
    public void ThenStdoutShouldBeEmpty() => Assert.Empty(Stdout);

    [Then("stderr should mention {string}")]
    public void ThenStderrShouldMention(string text) => Assert.Contains(text, Stderr);

    [Then("stderr should be the line {string}")]
    public void ThenStderrShouldBeTheLine(string line) => Assert.Equal(line + Environment.NewLine, Stderr);
}
