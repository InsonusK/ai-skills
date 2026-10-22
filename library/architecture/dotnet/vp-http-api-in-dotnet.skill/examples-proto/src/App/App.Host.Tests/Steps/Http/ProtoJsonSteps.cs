using App.Host.Api.Http.Pipeline;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Reqnroll;
using Shared.Errors;

namespace App.Host.Tests.Steps.Http;

/// <summary>
/// Test routes over a generated message of the protobuf runtime
/// (google.protobuf.Method: request_type_url in, response_type_url out), so
/// ProtoJson is tested without the application's contract.
/// </summary>
[Binding]
public sealed class ProtoJsonSteps(HttpWorld world)
{
    [Given("the HTTP routes read and write proto messages")]
    public void GivenTheHttpRoutesReadAndWriteProtoMessages() => world.Routes = (routes, sender) =>
    {
        routes.MapPost("/v1/proto", async (HttpRequest request, CancellationToken cancellationToken) =>
        {
            var method = await ProtoJson.ReadAsync<Method>(request);
            var answer = await sender.Send(new Echo(method.RequestTypeUrl), cancellationToken);
            return ProtoJson.Result(new Method { ResponseTypeUrl = answer });
        });
        routes.MapPost("/v1/old", () => { throw new ErrorKindException(ErrorKind.Unimplemented, "Old is deprecated since 1.1.0"); });
    };
}
