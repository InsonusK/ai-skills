using System.Text.Json;
using App.Host.Api.Http.Pipeline;
using Microsoft.AspNetCore.Routing;
using Reqnroll;
using Xunit;
using Xunit.Abstractions;

namespace App.Host.Tests.Steps.Http;

/// <summary>The routes the application maps held against the operations of the spec generated from its proto file.</summary>
[Binding]
public sealed class ContractSteps(HttpWorld world, ITestOutputHelper output)
{
    [Then("the HTTP API and its OpenAPI contract both have exactly the operations:")]
    public async Task ThenTheHttpApiAndItsContractHaveExactlyTheOperations(DataTable table)
    {
        var listed = table.Rows.Select(row => $"{row["method"]} {row["path"]}").Order().ToList();

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
        Assert.Equal(listed, contract);
        Assert.Equal(listed, served);
    }
}
