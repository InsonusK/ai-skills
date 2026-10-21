using Reqnroll;
using Xunit;

namespace App.Host.Tests.Steps;

/// <summary>How the process (or one run of the entry points) ended; shared by the steps of one scenario.</summary>
public sealed class ProcessOutcome
{
    public int? ExitCode { get; set; }
}

[Binding]
public sealed class ProcessOutcomeSteps(ProcessOutcome outcome)
{
    [Then("the exit code should be {int}")]
    public void ThenTheExitCodeShouldBe(int code) => Assert.Equal(code, outcome.ExitCode);
}
