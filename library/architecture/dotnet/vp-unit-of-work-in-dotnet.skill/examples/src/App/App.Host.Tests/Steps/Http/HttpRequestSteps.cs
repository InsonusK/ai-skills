using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Reqnroll;
using Shared.Errors;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>Requests to the HTTP API and what it answered; the echo command's fakes.</summary>
[Binding]
public sealed class HttpRequestSteps(HttpWorld world, ITestOutputHelper output)
{
    [Given("the echo command answers {string}")]
    public void GivenTheEchoCommandAnswers(string answer) => world.Echo.Answer = answer;

    [Given("the echo command is rejected with {string}")]
    public void GivenTheEchoCommandIsRejected(string message) => world.Echo.Failure = new ErrorKindException(ErrorKind.Invalid, message);

    [Given("the echo command fails with the kind {string} and {string}")]
    public void GivenTheEchoCommandFailsWithTheKind(string kind, string message) =>
        world.Echo.Failure = new ErrorKindException(Enum.Parse<ErrorKind>(kind.Replace(" ", ""), ignoreCase: true), message);

    [Given("the echo command fails with {string}")]
    public void GivenTheEchoCommandFails(string message) => world.Echo.Failure = new InvalidOperationException(message);

    [Given("the echo command is wrapped with logging")]
    public void GivenTheEchoCommandIsWrappedWithLogging() => world.WithLogging = true;

    private async Task Send(HttpMethod method, string path, string? body, string? contentType, string? requestId = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = new StringContent(body, Encoding.UTF8);
            request.Content.Headers.ContentType = contentType is null ? null : new MediaTypeHeaderValue(contentType);
        }
        if (requestId is not null)
        {
            request.Headers.Add("X-Request-Id", requestId);
        }
        await world.SendAsync(request);
        output.WriteLine($"when: {method} {path} {body} -> {(int)world.Response!.StatusCode} {world.ResponseBody}");
    }

    [When("I POST {string} to the HTTP API with the body {string}")]
    public Task WhenIPost(string path, string body) => Send(HttpMethod.Post, path, body, "application/json");

    [When("I POST {string} to the HTTP API with the body {string} and the request id {string}")]
    public Task WhenIPostWithTheRequestId(string path, string body, string requestId) => Send(HttpMethod.Post, path, body, "application/json", requestId);

    [When("I POST {string} to the HTTP API with the body {string} as {string}")]
    public Task WhenIPostAs(string path, string body, string contentType) => Send(HttpMethod.Post, path, body, contentType);

    [When("I GET {string} from the HTTP API")]
    public Task WhenIGet(string path) => Send(HttpMethod.Get, path, null, null);

    [Then("the HTTP status should be {int}")]
    public void ThenTheHttpStatusShouldBe(int status) => Assert.Equal(status, (int)world.Response!.StatusCode);

    [Then("the HTTP response body should be {string}")]
    public void ThenTheHttpResponseBodyShouldBe(string body) => Assert.Equal(body, world.ResponseBody);

    [Then("the HTTP response body should mention {string}")]
    public void ThenTheHttpResponseBodyShouldMention(string text) => Assert.Contains(text, world.ResponseBody);

    private (int Code, string Message, int Details) Error()
    {
        var status = JsonDocument.Parse(world.ResponseBody).RootElement;
        return (status.GetProperty("code").GetInt32(), status.GetProperty("message").GetString()!, status.GetProperty("details").GetArrayLength());
    }

    [Then("the HTTP error should have code {int} and a message containing {string}")]
    public void ThenTheHttpErrorShouldHaveCodeAndAMessageContaining(int code, string text)
    {
        var error = Error();
        Assert.Equal(code, error.Code);
        Assert.Contains(text, error.Message);
        Assert.Equal(0, error.Details);
    }

    [Then("the HTTP error should have code {int} and the message {string}")]
    public void ThenTheHttpErrorShouldHaveCodeAndTheMessage(int code, string message)
    {
        var error = Error();
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
    }

    [Then("the echo command should have received {string}")]
    public void ThenTheEchoCommandShouldHaveReceived(string text) => Assert.Equal(text, world.Echo.Received);

    [Then("the echo command should not have been called")]
    public void ThenTheEchoCommandShouldNotHaveBeenCalled() => Assert.Null(world.Echo.Received);
}
