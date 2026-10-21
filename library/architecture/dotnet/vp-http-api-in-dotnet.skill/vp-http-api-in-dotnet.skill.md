---
name: vp-http-api-in-dotnet
description: The HTTP API variation point in dotnet — Api/Http/Pipeline copied from templates (settings, the server on its own WebApplication, the request log, the error answers as google.rpc.Status, the docs), one {Resource}Handler class per resource in Api/Http/Endpoint written as Minimal API routes, the registration in Api/Http/Routes.cs, one http Entry in Program.cs, and the contract either hand-written in OpenAPI with routes over records of their own, or, as the HTTP form of a gRPC contract, generated from the proto file's google.api.http annotations with the routes over the generated messages (ProtoJson)
whenToUse: when a dotnet application built on the base plateau must be served over HTTP, when a dotnet service with a gRPC contract needs its HTTP form, when you serve a new command over HTTP in such an application, or when you review its Api/Http folders
updated: 20261021
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
adr:
  - adr/minimal-api-with-binding-and-one-error-middleware.md
  - adr/http-form-by-routes-over-generated-messages.md
---

# Goal
- **The HTTP variation point in dotnet** - A dotnet application that keeps every property of [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example; as the HTTP form of a proto contract, only those [attach-proto.allow](./attach-proto.allow) names against [vp-grpc-api-in-dotnet](library/architecture/dotnet/vp-grpc-api-in-dotnet.skill/vp-grpc-api-in-dotnet.skill.md)'s example.

# Core Principle
- **One contract, two forms** - With a gRPC contract, the proto file is the only contract: the routes read and write its generated messages and the spec is generated from its annotations by the Go catalog's generator, so the HTTP and gRPC forms — and a Go and a dotnet service of one contract — cannot describe different operations, statuses, or shapes. Decision recorded in [http-form-by-routes-over-generated-messages](./adr/http-form-by-routes-over-generated-messages.md).
- **A route is written for its success** - A route binds its request type, sends one command, and returns `TypedResults`; every error it lets out is answered once, by the pipeline's `ErrorAnswers`, as `google.rpc.Status`. Decision recorded in [minimal-api-with-binding-and-one-error-middleware](./adr/minimal-api-with-binding-and-one-error-middleware.md).
- **Copied once, written per command** - The entry point is split as [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md#split-each-entry-point-into-pipeline-endpoint-and-registration) requires: `Pipeline/` is copied from templates and never edited; per command the application writes, in its resource's class of `Endpoint/`, a route line, a method, and a request and a response record.

```
src/App/App.Host/Api/
├── Errors/RpcStatus.cs        copied from error-kinds-in-dotnet; the same in every network entry point
├── Hosting/WebHost.cs         copied; the same in every web entry point (HTTP, management, gRPC)
└── Http/
    ├── Pipeline/              copied from templates, never edited
    │   ├── HttpConfig.cs  HttpServer.cs  HttpDocs.cs
    │   ├── RequestLog.cs      the request log: received / sent, request id
    │   ├── ErrorAnswers.cs    every error as google.rpc.Status
    │   └── ProtoJson.cs       only for a proto contract: the generated messages in proto-JSON
    ├── Endpoint/              written per application, grows with the API
    │   └── ItemsHandler.cs    one class per resource ≈ a controller
    └── Routes.cs              HttpEndpoints (one parameter per resource), Map, HttpApi.RunAsync
docs/integration/api/{contract-name}/openapi.json    the hand-written contract, embedded into App.Host
  — or, for a proto contract: {file}.proto and {file}.swagger.json generated beside it (make proto-gen)
```

# Rule

## MUST

### Copy the entry point from templates
Copy, unchanged, [HttpConfig.cs](./templates/src/App/App.Host/Api/Http/Pipeline/HttpConfig.cs), [HttpServer.cs](./templates/src/App/App.Host/Api/Http/Pipeline/HttpServer.cs), [RequestLog.cs](./templates/src/App/App.Host/Api/Http/Pipeline/RequestLog.cs), [ErrorAnswers.cs](./templates/src/App/App.Host/Api/Http/Pipeline/ErrorAnswers.cs), and [HttpDocs.cs](./templates/src/App/App.Host/Api/Http/Pipeline/HttpDocs.cs) to `src/App/App.Host/Api/Http/Pipeline/`, [WebHost.cs](./templates/src/App/App.Host/Api/Hosting/WebHost.cs) to `Api/Hosting/`, and `RpcStatus.cs` of [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md) to `Api/Errors/`, unless another entry point already put the last two there.
- Risk: a retyped server drifts in its error answers, its request log, or its shutdown from every other dotnet service of the catalog.
- Fix: copy the files; they hold no placeholder — the namespaces are the same in every application, and Swagger UI's title is the contract's `info.title`.

### Reference ASP.NET Core and embed the contract
In `App.Host.csproj`, add `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, remove the `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` package references the framework now brings, add `Swashbuckle.AspNetCore.SwaggerUI` (10.3.1 in `Directory.Packages.props`), and embed the contract as `<EmbeddedResource Include="..\..\..\docs\integration\api\{contract-name}\openapi.json" LogicalName="openapi.json" />`.
- Violation: `Sdk="Microsoft.NET.Sdk.Web"` with its `appsettings.json` and launch profiles; `Swashbuckle.AspNetCore` with its generator; the spec read from disk at run time.
- Risk: a generated spec describes the code rather than the contract the clients were promised; a spec read from disk is missing from the container image.
- Fix: the Swagger UI package serves the UI only; `HttpDocs` reads the embedded file under its logical name.

### One class per resource in Endpoint
Write each resource of the API — the first path segment after the version, its collection and item paths together (`/v1/users` and `/v1/users/{id}` in `UsersHandler`) — as `sealed class {Resource}Handler(ISender sender)` in `Api/Http/Endpoint/{Resource}Handler.cs`, with `public void Map(IEndpointRouteBuilder routes)` that maps its group (`routes.MapGroup("/v1/users")`) and one route per command, a private method per route that binds its request record, sends one command, and returns `TypedResults.Ok`, `Created`, or `Accepted`, and the route's request and response records in the same file.
- Violation: a route that returns a command's result type; a `try/catch` or `Results.BadRequest` in a route; a route that takes a handler or a domain type; every resource's routes in one file.
- Risk: an encoded command type ties the published contract to the command layer; a route answering its own errors answers differently from the table; one routes file grows with every command until no one reads it.
- Fix: follow the example's [ItemsHandler.cs](./examples/src/App/App.Host/Api/Http/Endpoint/ItemsHandler.cs); a bound record of the route's own (`PlanItemsRequest`), mapped to the command and back from the result.

### Register the resources in Routes.cs
Keep in `Api/Http/Routes.cs` only `sealed record HttpEndpoints({Resource}Handler {Resource}, ...)`, its `Map`, which calls each resource's `Map`, and `HttpApi.RunAsync(endpoints, loggers, stopping)`, which serves them.
- Risk: a resource left out of `Map` is served by nothing while its class and its scenarios pass.
- Fix: follow the example's [Routes.cs](./examples/src/App/App.Host/Api/Http/Routes.cs); the contract scenario holds `Map` against the spec.

### Hand-write the contract
Write the contract as `docs/integration/api/{contract-name}/openapi.json` (OpenAPI 3) per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md), listing for every route its success status, 400 and 500 with the `Status` schema.
- Risk: a route missing from the spec is invisible in Swagger UI; a spec operation with no route answers 404.
- Fix: change the resource's class and `openapi.json` in the same change and bump `info.version`.

### Generate the spec of a proto contract from its annotations
Give every rpc of the proto file a `google.api.http` annotation and an `openapiv2_operation` listing its success status, each error status it can answer, and `default`, all with `.google.rpc.Status` as the error schema, as the [proto example](./examples-proto/docs/integration/api/todo/todo.proto) does; vendor the imported `google/api/{annotations,http}.proto` and `protoc-gen-openapiv2/options/{annotations,openapiv2}.proto` under `third_party/proto/`, compile them with the contract (`AdditionalImportDirs`, a `<Protobuf ... GrpcServices="None">` item), and generate `{file}.swagger.json` with the example's `make proto-gen` — `protoc` of the `Grpc.Tools` package with the `protoc-gen-openapiv2` v2.31.0 release binary, `disable_default_responses=true` and `enable_rpc_deprecation=true` — committing it and embedding it as `openapi.json`.
- Violation: a hand-written `openapi.json` beside the proto file; `Microsoft.AspNetCore.Grpc.Swagger`; another version or other options of the generator.
- Risk: two descriptions of one API drift; a spec generated at run time lists 200 for every method and marks nothing deprecated; another generator than the Go catalog's publishes a different spec for the same proto file.
- Fix: `make proto-gen` after every proto change; the example's spec is byte for byte the Go example's.

### Map the generated messages with ProtoJson
Copy [ProtoJson.cs](./templates/src/App/App.Host/Api/Http/Pipeline/ProtoJson.cs) beside the other templates, and write each route of a proto contract as a method taking its path parameters (named as the spec names them, `listId`) and `HttpRequest`, reading the body with `await ProtoJson.ReadAsync<{Rpc}Request>(request)`, sending one command, and answering `ProtoJson.Result(response, StatusCodes.Status201Created)` with the generated response message and the success status of its `openapiv2_operation`.

```csharp
// rpc AddItem - POST /v1/lists/{listId}/items, 201
private async Task<IResult> AddItemAsync(string listId, HttpRequest request, CancellationToken cancellationToken)
{
    var body = await ProtoJson.ReadAsync<AddItemRequest>(request);
    var result = await sender.Send(new AddItemCommand(listId, body.Title), cancellationToken);
    return ProtoJson.Result(new AddItemResponse { Item = new Item { Title = result.Item.Title } }, StatusCodes.Status201Created);
}
```

- Violation: a record of the route's own beside a generated message; Minimal API binding or `TypedResults` on a generated message; a path parameter `{list_id}` while the spec says `{listId}`.
- Risk: System.Text.Json writes and reads a generated message other than the proto-JSON mapping (field names, `null`, 64-bit integers), so the HTTP form differs from what a grpc-gateway or the Go service answers; a route pattern that differs from the spec fails the contract scenario.
- Fix: proto-JSON in and out — a field by its lowerCamelCase or proto name, an unknown field ignored, a field at its default left out of a response.

### Keep each path parameter a whole segment
Write every `google.api.http` path with each variable as a whole segment (`/v1/lists/{list_id}/clear`), never a custom-method suffix (`/v1/lists/{list_id}:clear`).
- Risk: the Go catalog's `http.ServeMux` cannot register a wildcard that is not a whole segment, so the same contract cannot be served by both stacks.
- Fix: `/clear` in place of `:clear`.

### Answer a deprecated rpc 501 in both forms
Mark an rpc that is kept but no longer served `option deprecated = true`, and answer it in both forms by throwing `new ErrorKindException(ErrorKind.Unimplemented, "{Rpc} is deprecated since {version}")` with the same message, calling no command; mark the gRPC override `[Obsolete(...)]`, as the generated base method is.
- Risk: a removed route answers 404 and a removed method fails the build of every client that still has it; two messages for one method confuse the client.
- Fix: keep the rpc until the next major version; the table answers 501 / `UNIMPLEMENTED`.

### Serve both forms from the same handlers
In a service with the gRPC variation point, add the `http` entry beside the `grpc` one in `// [entries]`, both built with the same `ISender`.

```csharp
var grpcEndpoints = new GrpcEndpoints(Items: new GrpcEndpoint.ItemsHandler(sender));
var httpEndpoints = new HttpEndpoints(Items: new HttpEndpoint.ItemsHandler(sender), Lists: new HttpEndpoint.ListsHandler(sender));
var error = await EntryRunner.RunAllAsync(logger, stopping,
    new Entry("grpc", stopping => GrpcApi.RunAsync(grpcEndpoints, loggers, stopping)),
    new Entry("http", stopping => HttpApi.RunAsync(httpEndpoints, loggers, stopping)));
```

- Risk: a form that reaches the commands another way runs a different chain of behaviors.
- Fix: both forms send through the one `ISender`; the two `ItemsHandler` classes are told apart by a namespace alias in `Program.cs`.

### Replace the startup entry in Program.cs
Replace the startup `Entry` in `// [entries]` of `Program.cs` with an `http` one calling `HttpApi.RunAsync` with `HttpEndpoints` built from `ISender`, and delete `Api/Startup/` with its feature and steps.

```csharp
// [entries] - every entry point, one Entry each, run together by EntryRunner.
var loggers = provider.GetRequiredService<ILoggerFactory>();
var httpEndpoints = new HttpEndpoints(Items: new ItemsHandler(sender));
var error = await EntryRunner.RunAllAsync(logger, stopping,
    new Entry("http", stopping => HttpApi.RunAsync(httpEndpoints, loggers, stopping)));
return EntryRunner.ExitCode(error);
```

- Violation: the settings loaded or the `WebApplication` built in `Program.cs`; `HttpApi.RunAsync` awaited outside `RunAllAsync`; the startup entry kept beside it.
- Risk: every line of the entry point's start in `Program.cs` is a line each later variation point's instruction must step around; a server started outside the list is not stopped when another entry point fails.
- Fix: `HttpApi.RunAsync` reads the settings, builds the server, and serves until `stopping` is cancelled; other entries of the list (health, gRPC) stay as they are.

### Cover the entry point with scenarios
Copy the example's `src/App/App.Host.Tests/features/Http/` scenarios of the fixed code (`Endpoint`, `RequestLog`, `HttpConfig`, `HttpServe`, `Docs`) with their steps (`Steps/Http/HttpWorld.cs`, `HttpRequestSteps`, `RequestLogSteps`, `HttpConfigSteps`, `HttpServeSteps`, `DocsSteps`), `Steps/CapturedLogs.cs`, `Steps/FreePort.cs`, and `features/RpcStatus.feature` with its steps, and add `Microsoft.AspNetCore.TestHost` to the test project; write one feature per resource with its handlers faked (a `IRequestHandler` registered in `HttpWorld.Handlers`) and the contract scenario; tag them `@type/api/http` and `@type/tech-check`.
- Violation: a step text without "HTTP" in it (`the status should be 200`); a resource feature that repeats the error answers.
- Risk: every entry point of `App.Host` shares one test project, so a step text two variation points both define makes Reqnroll fail as ambiguous once both are attached; a resource feature that repeats the error mapping grows the suite with every resource and tests nothing new.
- Fix: name the entry point in every step (`the HTTP status should be 200`, `the management status should be 200`); a resource feature checks only its own binding and mapping; `make test` passes.

## SHOULD

### Serve a new command in its resource's class
Serve a further command of an existing resource by adding its route line to `Map`, its method, and its request and response records to the resource's class, its path to `openapi.json`, and its scenario to the resource's feature; a new resource adds its class and feature, one parameter of `HttpEndpoints`, and one line of `Map`.

# Check list
- [ ] `Api/Http/Pipeline/{HttpConfig,HttpServer,RequestLog,ErrorAnswers,HttpDocs}.cs`, `Api/Hosting/WebHost.cs`, and `Api/Errors/RpcStatus.cs` are copies of the templates.
- [ ] `App.Host.csproj` references `Microsoft.AspNetCore.App` and `Swashbuckle.AspNetCore.SwaggerUI` and embeds the contract as `openapi.json`.
- [ ] Each resource is one `{Resource}Handler` class in `Api/Http/Endpoint/` mapping its group; every route binds a record of its own, sends one command, and returns `TypedResults` with no error handling of its own.
- [ ] `Routes.cs` holds only `HttpEndpoints`, its `Map`, and `HttpApi.RunAsync`.
- [ ] `docs/integration/api/{contract-name}/` holds one spec: the hand-written `openapi.json`, or, for a proto contract, the generated `{file}.swagger.json`; the contract scenario passes.
- [ ] For a proto contract: every rpc has `google.api.http` and `openapiv2_operation`; `make proto-gen` leaves no diff; the routes use the generated messages through `ProtoJson` and the spec's path-parameter names; every path variable is a whole segment; a deprecated rpc answers 501 / `UNIMPLEMENTED` in both forms.
- [ ] `// [entries]` has an `http` entry calling `HttpApi.RunAsync`; `Api/Startup/` is deleted.
- [ ] Every step text names its entry point; `make test` passes.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow) and [attach-proto.allow](./attach-proto.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the HTTP API attached: `make run`, `make test`.
- [examples-proto/](./examples-proto/README.md) — the HTTP form of the gRPC example's proto contract, with a path parameter, 201, 404, 409, a deprecated rpc, and the first service and infrastructure (lists kept in memory): `make run`, `make proto-gen`, `make test`.
