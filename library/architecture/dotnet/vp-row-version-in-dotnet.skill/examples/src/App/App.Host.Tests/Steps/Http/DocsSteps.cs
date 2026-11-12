using System.Text.Json;
using App.Host.Api.Http.Pipeline;
using Reqnroll;
using Xunit;

namespace App.Host.Tests.Steps.Http;

[Binding]
public sealed class DocsSteps(HttpWorld world)
{
    [Given("the HTTP docs are enabled")]
    public void GivenTheHttpDocsAreEnabled() => world.Config = world.Config with { DocsEnabled = true };

    [Given("the service is published under {string}")]
    public void GivenTheServiceIsPublishedUnder(string basePath) => world.Config = world.Config with { BasePath = basePath };

    // basePath in an OpenAPI 2 spec generated from a proto file, servers[0].url in an OpenAPI 3 one.
    [Then("the spec's server URL should be {string}")]
    public void ThenTheSpecsServerUrlShouldBe(string url)
    {
        var spec = JsonDocument.Parse(world.ResponseBody).RootElement;
        Assert.Equal(url, spec.TryGetProperty("swagger", out _)
            ? spec.GetProperty("basePath").GetString()
            : spec.GetProperty("servers")[0].GetProperty("url").GetString());
    }

    // The Location, resolved against the URL the client called (the published
    // prefix and the path asked for), lands under the prefix: a relative one
    // resolves under any prefix the ingress strips.
    [Then("the HTTP response should redirect into {string}")]
    public void ThenTheHttpResponseShouldRedirectInto(string prefix)
    {
        Assert.InRange((int)world.Response!.StatusCode, 300, 399);
        var called = new Uri($"http://client{world.Config.BasePath}{world.Response.RequestMessage!.RequestUri!.AbsolutePath}");
        var landed = new Uri(called, world.Response.Headers.Location!).AbsolutePath;
        Assert.StartsWith(prefix, landed);
    }

    // Swagger UI's page, then the script that tells it where the spec is.
    [When("I open the Swagger UI of the HTTP API")]
    public async Task WhenIOpenTheSwaggerUi()
    {
        await world.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/swagger/index.html"));
        Assert.Equal(200, (int)world.Response!.StatusCode);
        await world.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/swagger/index.js"));
        Assert.Equal(200, (int)world.Response!.StatusCode);
    }

    [Then("the Swagger UI should load the spec from {string}")]
    public void ThenTheSwaggerUiShouldLoadTheSpecFrom(string url) => Assert.Contains($"\"{url}\"", world.ResponseBody);

    [Then("the HTTP response should be the embedded spec")]
    public void ThenTheHttpResponseShouldBeTheEmbeddedSpec() => Assert.Equal(HttpDocs.PublishedSpec(""), world.ResponseBody);
}
