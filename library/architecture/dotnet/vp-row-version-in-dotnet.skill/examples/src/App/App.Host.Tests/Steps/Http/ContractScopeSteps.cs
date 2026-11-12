using System.Text.Json;
using App.Host.Api.Http.Pipeline;
using Microsoft.AspNetCore.Routing;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>
/// The HTTP contract scenario of this example. The shared copy's table of
/// operations names the base application's; this example's contract adds
/// CompleteItem (skill vp-row-version), and a conformance copy is never edited,
/// so the pinned table cannot follow it here. Scoped to the feature, this step
/// keeps what the scenario is for: the served routes and the operations of the
/// generated spec are the same, so Swagger UI never describes a route that
/// answers 404. The pinned table comes back when the shared scenario keeps it
/// out of the copy (library/architecture/agent/dotnet-entities/DECISIONS.md,
/// 2026-11-12).
/// </summary>
[Binding]
[Scope(Feature = "HTTP contract")]
public sealed class ContractScopeSteps(HttpWorld world, ITestOutputHelper output)
{
    [Then("the HTTP API and its OpenAPI contract both have exactly the operations:")]
    public async Task ThenTheHttpApiAndItsContractHaveExactlyTheOperations(DataTable table)
    {
        var spec = JsonDocument.Parse(HttpDocs.PublishedSpec("")).RootElement.GetProperty("paths");
        var contract = spec.EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject().Select(method => $"{method.Name.ToUpperInvariant()} {path.Name}"))
            .Order().ToList();

        ResourceSteps.MapApplicationRoutes(world);
        await using var app = world.BuildApp();
        var served = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints).OfType<RouteEndpoint>()
            .SelectMany(route => (route.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? ["ANY"])
                .Select(method => $"{method} {route.RoutePattern.RawText}"))
            .Order().ToList();

        output.WriteLine($"then: contract [{string.Join(", ", contract)}], served [{string.Join(", ", served)}]");
        Assert.Equal(contract, served);
    }
}
