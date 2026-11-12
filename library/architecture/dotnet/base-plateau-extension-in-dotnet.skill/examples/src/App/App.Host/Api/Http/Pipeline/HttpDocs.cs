// Source: skill vp-http-api-in-dotnet

using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace App.Host.Api.Http.Pipeline;

/// <summary>
/// The OpenAPI spec and Swagger UI, served only when HTTP_API_DOCS_ENABLED is
/// true. The spec is the contract file the project embeds under the logical
/// name <see cref="SpecResource"/>; its info.title is Swagger UI's title.
/// Mounted at "/" like every route; each URL they hand to a browser carries
/// the base path.
/// </summary>
public static class HttpDocs
{
    /// <summary>The logical name of the embedded contract (App.Host.csproj: EmbeddedResource LogicalName).</summary>
    public const string SpecResource = "openapi.json";

    /// <summary>
    /// The embedded contract with the path the service is published under set
    /// in it, so Swagger UI's "Try it out" calls this service and not the
    /// ingress root: servers[0].url in an OpenAPI 3 spec, basePath in an
    /// OpenAPI 2 spec generated from a proto file. The committed file names neither.
    /// </summary>
    public static string PublishedSpec(string basePath)
    {
        using var stream = typeof(HttpDocs).Assembly.GetManifestResourceStream(SpecResource)
            ?? throw new InvalidOperationException($"the contract is not embedded as \"{SpecResource}\"");
        var embedded = new StreamReader(stream).ReadToEnd();
        if (basePath == "")
        {
            return embedded;
        }
        var spec = JsonNode.Parse(embedded)!.AsObject();
        if (spec.ContainsKey("swagger"))
        {
            spec["basePath"] = basePath;
        }
        else
        {
            spec["servers"] = new JsonArray(new JsonObject { ["url"] = basePath });
        }
        return spec.ToJsonString();
    }

    /// <summary>Adds GET /openapi.json and Swagger UI under /swagger.</summary>
    public static void Map(WebApplication app, string basePath)
    {
        var spec = PublishedSpec(basePath);
        var title = JsonNode.Parse(spec)?["info"]?["title"]?.GetValue<string>() ?? "API";
        app.MapGet("/openapi.json", () => Results.Text(spec, "application/json"));
        app.UseSwaggerUI(ui =>
        {
            ui.RoutePrefix = "swagger";
            ui.DocumentTitle = title;
            ui.SwaggerEndpoint(basePath + "/openapi.json", title);
        });
    }
}
