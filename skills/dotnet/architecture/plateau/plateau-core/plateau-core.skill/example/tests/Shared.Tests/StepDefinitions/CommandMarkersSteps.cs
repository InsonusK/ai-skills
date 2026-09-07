using System.Reflection;
using Reqnroll;
using Shared.MediatR;
using Xunit;

namespace Shared.Tests.StepDefinitions;

[Binding]
public sealed class CommandMarkersSteps
{
    private Type[] _markers = [];

    [When("the request markers are inspected")]
    public void WhenInspected() =>
        _markers = [typeof(ICommand), typeof(ICommand<>), typeof(IQuery<>), typeof(INotificationEvent)];

    [Then("each one is in namespace {string}")]
    public void ThenNamespace(string ns) =>
        Assert.All(_markers, t => Assert.Equal(ns, t.Namespace));

    [Then("each one declares no instance members")]
    public void ThenNoMembers() =>
        Assert.All(_markers, t => Assert.Empty(
            t.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)));
}
