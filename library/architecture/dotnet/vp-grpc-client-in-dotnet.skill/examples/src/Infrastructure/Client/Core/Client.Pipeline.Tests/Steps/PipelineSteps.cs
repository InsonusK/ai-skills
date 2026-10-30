using Reqnroll;
using Xunit;

namespace Client.Pipeline.Tests.Steps;

[Binding]
public sealed class PipelineSteps
{
    private readonly List<IClientMiddleware> _middlewares = [];
    private readonly List<string> _steps = [];

    /// <summary>A middleware that records its steps; "twice" calls next twice.</summary>
    private sealed class Recording(string name, List<string> steps) : IClientMiddleware
    {
        public async Task RunAsync(ClientCall call, ClientAttempt next, CancellationToken cancellationToken)
        {
            steps.Add(name + " before");
            await next(cancellationToken);
            if (name == "twice")
            {
                await next(cancellationToken);
            }
            steps.Add(name + " after");
        }
    }

    [Given("the client middleware {string}")]
    public void GivenTheClientMiddleware(string names) =>
        _middlewares.AddRange(names.Split('|').Select(name => new Recording(name, _steps)));

    [When("a call of {string} is run")]
    public Task WhenACallIsRun(string operation) =>
        ClientPipeline.RunAsync(new ClientCall("test-api", operation, false), _middlewares, _ =>
        {
            _steps.Add("attempt");
            return Task.CompletedTask;
        }, CancellationToken.None);

    [Then("the steps should be, in order, {string}")]
    public void ThenTheStepsShouldBe(string steps) => Assert.Equal(steps, string.Join("|", _steps));
}
