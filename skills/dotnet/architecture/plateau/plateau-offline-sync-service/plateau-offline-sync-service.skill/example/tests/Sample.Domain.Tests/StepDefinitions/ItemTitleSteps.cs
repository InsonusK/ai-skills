using Reqnroll;
using Sample.Domain.ValueObjects;
using Shared.Exceptions;
using Xunit;

namespace Sample.Domain.Tests.StepDefinitions;

// The @format scenarios from Sample.Domain.Rules.Spec/ItemTitle.feature, re-proven fail-fast
// through the ItemTitle constructor. Same Gherkin text the rule's own ItemTitleRuleSteps binds
// in Sample.Domain.Rules.Tests — Reqnroll resolves bindings per assembly, so each project binds
// the identical scenario to its own adapter. A client in another language does the same.
[Binding]
public sealed class ItemTitleSteps
{
    private DomainException? _error;

    [When("the title {string} is checked")]
    public void WhenChecked(string value)
        => _error = Record.Exception(() => new ItemTitle(value)) as DomainException;

    [When("a 101-character title is checked")]
    public void WhenLong()
        => _error = Record.Exception(() => new ItemTitle(new string('x', 101))) as DomainException;

    [Then("the check fails with error code {string}")]
    public void ThenFails(string code)
    {
        Assert.NotNull(_error);
        Assert.Equal(code, _error!.Code);
    }

    [Then("the check passes")]
    public void ThenPasses() => Assert.Null(_error);
}
