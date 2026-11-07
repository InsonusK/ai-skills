using Reqnroll;
using Shared.Errors;
using Xunit;

namespace Todo.Application.Tests.Common.Steps;

/// <summary>
/// The failure of the command a scenario sent, shared by every step class of
/// the scenario: Reqnroll gives each one the same instance.
/// </summary>
public sealed class CommandOutcome
{
    public Exception? Error { get; private set; }

    /// <summary>Runs <paramref name="call"/>, keeping its failure instead of throwing it.</summary>
    public async Task AttemptAsync(Func<Task> call)
    {
        Error = null;
        try
        {
            await call();
        }
        catch (Exception error)
        {
            Error = error;
        }
    }

    /// <summary>The kind the scenarios name ("not found"), as an <see cref="ErrorKind"/>.</summary>
    public static ErrorKind KindNamed(string name) => Enum.Parse<ErrorKind>(name.Replace(" ", ""), ignoreCase: true);
}

/// <summary>The steps on a command's failure, the same for every command.</summary>
[Binding]
public sealed class CommandOutcomeSteps(CommandOutcome outcome)
{
    [Then("the command should fail with the kind {string}")]
    public void ThenTheCommandShouldFailWithTheKind(string kind) =>
        Assert.Equal(CommandOutcome.KindNamed(kind), ErrorKindException.KindOf(outcome.Error));
}
