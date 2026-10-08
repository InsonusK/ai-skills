using Reqnroll;
using Xunit;
using Xunit.Abstractions;
using Linkcheck;
namespace Linkcheck.Tests;
[Binding]
public sealed class CheckSteps(World world, ITestOutputHelper log)
{
    [Given("the URL {string}")] public void Url(string url) { world.Url = url; log.WriteLine($"URL: {url}"); }
    [Given("I check the URL")] [When("I check the URL")] public void Check() { world.Result = Checker.Check(world.Url); log.WriteLine($"Check: {world.Result}"); }
    [Then("the check is valid")] public void Valid() { log.WriteLine($"Valid: {world.Result.IsValid}"); Assert.True(world.Result.IsValid); }
    [Then("the normalized URL is {string}")] public void Normalized(string expected) { log.WriteLine(world.Result.Normalized); Assert.Equal(expected, world.Result.Normalized); }
    [Then("the check is invalid with error {string}")] public void Invalid(string expected) { log.WriteLine(world.Result.ErrorCode); Assert.False(world.Result.IsValid); Assert.Equal(expected, world.Result.ErrorCode); }
    [Then("the error code is empty")] public void EmptyError() { log.WriteLine(world.Result.ErrorCode); Assert.Equal("", world.Result.ErrorCode); }

}
