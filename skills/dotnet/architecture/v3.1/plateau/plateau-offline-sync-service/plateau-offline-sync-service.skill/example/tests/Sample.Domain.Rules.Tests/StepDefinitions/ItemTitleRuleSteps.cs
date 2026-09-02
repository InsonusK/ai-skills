using FluentValidation.Results;
using Reqnroll;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;
using Xunit;

namespace Sample.Domain.Rules.Tests.StepDefinitions;

[Binding]
public sealed class ItemTitleRuleSteps
{
    private ValidationResult _result = null!;

    [When("the title {string} is checked")]
    public void WhenChecked(string value) => _result = new SoftItemTitle(value).Check();

    [When("a 101-character title is checked")]
    public void WhenLong() => _result = new SoftItemTitle(new string('x', 101)).Check();

    [Then("the check fails with error code {string}")]
    public void ThenFails(string code)
    {
        Assert.False(_result.IsValid);
        Assert.Contains(_result.Errors, e => e.ErrorCode == code);
    }

    [Then("the check passes")]
    public void ThenPasses() => Assert.True(_result.IsValid);
}
