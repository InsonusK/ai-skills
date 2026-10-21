using App.Host.Api.Errors;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps;

[Binding]
public sealed class RpcStatusSteps(ITestOutputHelper output)
{
    private RpcCode _code;
    private string _message = "";

    private void Answer(Exception error)
    {
        (_code, _message) = RpcStatus.Of(error);
        output.WriteLine($"when: Of({error.GetType().Name}: {error.Message}) -> {_code} \"{_message}\"");
    }

    private static ErrorKindException Kinded(string kind, string message) =>
        new(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), message);

    [When("a command fails with the kind {string} and the message {string}")]
    public void WhenACommandFailsWithTheKind(string kind, string message) => Answer(Kinded(kind, message));

    [When("a command fails with the kind {string} and the message {string}, wrapped in {string}")]
    public void WhenACommandFailsWithTheKindWrapped(string kind, string message, string wrapper) =>
        Answer(new InvalidOperationException(wrapper, Kinded(kind, message)));

    [When("a command fails with the plain error {string}")]
    public void WhenACommandFailsWithThePlainError(string message) => Answer(new InvalidOperationException(message));

    [Then("the rpc code should be {int}")]
    public void ThenTheRpcCodeShouldBe(int code) => Assert.Equal(code, (int)_code);

    [Then("the rpc code's HTTP status should be {int}")]
    public void ThenTheRpcCodesHttpStatusShouldBe(int status) => Assert.Equal(status, _code.HttpStatus());

    [Then("the rpc message should be {string}")]
    public void ThenTheRpcMessageShouldBe(string message) => Assert.Equal(message, _message);

    [Then("the rpc code {int} should have the HTTP status {int}")]
    public void ThenTheRpcCodeShouldHaveTheHttpStatus(int code, int status) => Assert.Equal(status, ((RpcCode)code).HttpStatus());
}
