using Linkcheck;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace Linkcheck.Tests;

[Binding]
public sealed class CommandSteps(World world, ITestOutputHelper log)
{
    private readonly StringWriter output = new();
    private readonly StringWriter error = new();
    private int exitCode;

    [When("I run the command with {string}")]
    public void Run(string args)
    {
        exitCode = Command.Run(
            args.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            output,
            error
        );
        log.WriteLine($"Command exit: {exitCode}");
    }

    [Then("the exit code is {int}")]
    public void Exit(int expected)
    {
        log.WriteLine($"Exit: {exitCode}");
        Assert.Equal(expected, exitCode);
    }

    [Then("the output is:")]
    public void Output(Table table)
    {
        log.WriteLine(output.ToString());
        Assert.Equal(
            table.Rows.Select(r => r["line"]),
            output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        );
    }

    [Then("the error output starts with {string}")]
    public void Error(string prefix)
    {
        log.WriteLine(error.ToString());
        Assert.StartsWith(prefix, error.ToString());
    }
}
