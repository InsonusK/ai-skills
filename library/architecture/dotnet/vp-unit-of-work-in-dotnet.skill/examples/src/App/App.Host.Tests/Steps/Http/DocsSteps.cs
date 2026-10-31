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

    // Relative, so it resolves under any prefix the ingress strips.
    [Then("the HTTP response should redirect to the relative {string}")]
    public void ThenTheHttpResponseShouldRedirectTo(string location) => Assert.Equal(location, world.Response!.Headers.Location?.OriginalString);

    [Then("the HTTP response should be the embedded spec")]
    public void ThenTheHttpResponseShouldBeTheEmbeddedSpec() => Assert.Equal(HttpDocs.PublishedSpec(""), world.ResponseBody);
}
