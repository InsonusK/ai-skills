---
name: vp-http-client-in-dotnet
description: The HTTP client variation point in dotnet — Client.Abstractions (ApiUnavailableException, ApiStatusException, ApiStatusCode) and Client.Http (HttpApiConfig, HttpApiClient.CallAsync with the timeout, JSON in and out, the call log, the request id, the mock as an HttpMessageHandler) copied from templates, entity-focused clients in Client/{ApiName} with its {ApiName}.Client.Abstractions, its mock, and its contract scenario, the adapting implementation in Service/{ApiServiceName}/{ApiServiceName}.Service, and the lines of Program.cs that build them
whenToUse: when a dotnet application built on the base plateau must call another service's HTTP API, when you call a further operation of such an API, or when you review a dotnet HTTP client
updated: 20261128
adr:
  - adr/one-outcome-project-for-every-client.md
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The HTTP client variation point in dotnet** - A dotnet application that keeps every property of [vp-http-client](library/architecture/spec/vp-http-client.skill/vp-http-client.skill.md) and [api-client](library/develop/core/api-client.skill/api-client.skill.md), in the projects of [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md); this skill details their implementation for dotnet and restates none of their rules.
- **Attached by the listed changes only** - Attaching it adds projects and files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example.

# Core Principle
- **Copied once, written per operation** - `Client.Abstractions` and `Client.Http` are copied from templates and never edited; per API the application writes `{ApiName}.Client.Abstractions` with its interface, `{ApiName}.Client` with the client whose operations are calls of `HttpApiClient.CallAsync` and its mock, and the service over it.
- **The service sees outcomes, never a transport** - `{ApiServiceName}.Service` references `{ApiName}.Client.Abstractions`, which references `Client.Abstractions`: the service catches `ApiUnavailableException` and `ApiStatusException`, the same for an HTTP and a gRPC client. Decision recorded in [one-outcome-project-for-every-client](./adr/one-outcome-project-for-every-client.md).

```text
docs/integration/client/{api-name}/       provider's wire contract
src/Client/
  Core/{Client.Abstractions,Client.Pipeline,Client.Http}/
  {ApiName}/{ApiName}.Client.Abstractions/   logical API DTOs and I{ApiName}Client
  {ApiName}/{ApiName}.Client/                concrete protocol implementation and Mock/
  {ApiName}/{ApiName}.Client.Tests/          emulator and contract scenarios
src/Modules/{Module}/{Module}.Application/Services/I{Service}.cs
src/Service/{ApiServiceName}/{ApiServiceName}.Service/      adapting implementation and errors
src/Service/{ApiServiceName}/{ApiServiceName}.Service.Tests/ service tests with clients faked
```

# Rule

## MUST

### Distinguish delivered code from illustrations
Fill and copy explicitly delivered templates and fixed support files as instructed, and write application-specific client/service code for the selected API and consumer, treating snippets and application-specific files under `examples/` as illustrations.
- Violation: adopting an example's provider names, operations, DTOs or error meanings without deriving them from the target application's contracts.
- Risk: code compiles against the sample API while implementing the wrong application behavior.
- Fix: substitute only declared identifiers in delivered fixed code; derive application-specific operations, configuration prefixes, DTOs, Domain Model mapping and error meanings from the target contracts.

### Keep DTOs in the client abstraction project
Declare protocol-neutral API DTOs in `{ApiName}.Client.Abstractions` without transport-specific message or library references.
- Risk: service signatures and errors depend on the selected HTTP/gRPC implementation.
- Fix: keep the same interface and DTOs across transports; generated wire types and protocol error parsing remain in the implementation project.


### Fill the service namespace
Fill `{service-namespace}` with the application's namespace root from [the namespace convention](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md#qualify-namespaces-with-the-service-name) before copying this skill's templates.
- Risk: an unfilled identifier does not compile, while a different root in each file splits the service's types.
- Fix: substitute the same identifier in every delivered source, preserve template logic, and verify no `{service-namespace}` remains in application sources.

### Copy the client code from templates
Copy [Client.Abstractions](./templates/src/Client/Core/Client.Abstractions/), [Client.Pipeline](./templates/src/Client/Core/Client.Pipeline/), and [Client.Http](./templates/src/Client/Core/Client.Http/) to `src/Client/Core/`, with the example's `Client.Pipeline.Tests` and `Client.Http.Tests`, and add the five projects to the solution.
- Risk: a retyped client drifts in its timeout, its outcomes, or its log from every other dotnet client of the catalog.
- Fix: copy the files unchanged after namespace substitution; `make test` passes.

### Entity-focused client contracts and implementations
Write each entity-focused API client as two projects under `src/Client/{ApiName}/`: `{ApiName}.Client.Abstractions` with `I{ApiName}Client`, one method per operation in the API's terms and its own item records, referencing only `Client.Abstractions`; and `{ApiName}.Client` with `{ApiName}Client : I{ApiName}Client` holding `Prefix` (the selected `{API}` settings group), `Operations` (an `HttpOperation` - method, path, and `Retryable` where a repeat does what one call does - for every operation it calls), the client middleware in its constructor, and per operation one method that calls `HttpApiClient.CallAsync` with private request and response records and maps the answer.

```csharp
public async Task<IReadOnlyList<TodoItemDto>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
{
    var answer = await _http.CallAsync<PlanItemsResponse>(HttpMethod.Post, "/v1/items/plan", new PlanItemsRequest(titles), cancellationToken);
    return answer.Items.Select(item => new TodoItemDto(item.Title, item.Done)).ToList();
}
```

- Violation: `HttpClient` used in the client; `{ApiServiceName}.Service` referencing `{ApiName}.Client` itself; the client throwing an `ErrorKindException`; an operation missing from `Operations`.
- Risk: a call that bypasses `CallAsync` has no timeout, no log, and no request id; a service on the client's class cannot be tested with a fake.
- Fix: add each consumed operation to `I{ApiName}Client`, implement it in `{ApiName}Client` through the fixed transport code, register it in `Operations`, and implement the corresponding mock behavior.

### Answer every operation in the mock
Write the mock of each client as `src/Client/{ApiName}/{ApiName}.Client/Mock/{ApiName}Mock.cs`, an `HttpMessageHandler` that answers every operation of `Operations` as the API does, with stable data and no state, and pass it to `HttpApiClient` in the client; `{API}_HOST=mock` runs the client against it. The file's name says it is a mock and of which client.
- Risk: an operation the mock does not route answers 404 under `mock`, and the service cannot run without the other service.
- Fix: the mocked scenario of the client's feature runs it and checks its answer.

### Map the outcomes in the service
Write the adapting service in `src/Service/{ApiServiceName}/{ApiServiceName}.Service/` with its interface beside its consumer, per [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md), and its errors as factory methods of `{Service}Errors.cs` beside it: catch `ApiUnavailableException` as the kind unavailable, each `ApiStatusException` code it expects as the kind that fits, with the outcome as the inner exception, and let anything else pass with no kind; throw an entity's rule error raised by the API's answer as an exception with no inner exception.

```csharp
catch (ApiUnavailableException unavailable)
{
    throw RemotePlannerErrors.Unavailable(unavailable);
}
catch (ApiStatusException rejected) when (rejected.Code == ApiStatusCode.InvalidArgument)
{
    throw RemotePlannerErrors.TitlesRejected(rejected.StatusMessage, rejected);
}
```

- Violation: the `ErrorKindException` of an entity built from the answer passed as an inner exception, which `ErrorKindException.KindOf` finds.
- Risk: an item the other service got wrong is answered to this service's client as its own invalid input.
- Fix: convert provider-data invariant failures into an exception with no application kind and without the domain-kind exception as its inner exception; retain technical dependency failures as causes of the application errors they produce.

### Build the client and the service in Program.cs
Load the settings with `HttpApiConfig.Load({ApiName}Client.Prefix)` in `// [services]` of `Program.cs`, stopping the start with "startup failed" on a `FormatException`, and register the client as a singleton of `I{ApiName}Client` built with a logger of the logger factory, and the service as a singleton of its interface. Give every client the client middleware of its API, per [api-client](library/develop/core/api-client.skill/api-client.skill.md): build the list in `// [client middleware]` inside the client's registration - empty until a variation point (`vp-client-retry`) adds one - and pass it to the client's constructor.

```csharp
services.AddSingleton<ITodoClient>(provider =>
{
    var loggers = provider.GetRequiredService<ILoggerFactory>();
    // [client middleware]
    IReadOnlyList<IClientMiddleware> todoApiMiddlewares = [];
    return new TodoClient(todoApiConfig, loggers.CreateLogger("Client.Todo"), todoApiMiddlewares);
});
services.AddSingleton<IRemotePlanner, RemotePlannerService>();
```

- Risk: settings read when the client is first resolved fail the first command instead of the start; a client built in a handler cannot be mocked or configured from one place.
- Fix: the handler takes the service's interface; the composition root builds both.

### Cover the client with scenarios
Copy the example's `Client.Http.Tests` with the chosen service namespace: it covers the settings, the call, the outcomes, the log, and the mock through a test API of its own on `HttpListener`. Write per client an `{ApiName}.Client.Tests` calling each operation against an `HttpListener` of the scenario and its mock, with the contract scenario reading the copied `openapi.json` linked into its output; per service and per command scenarios with the layer below faked by hand; tag them `@type/tech-check` and `@type/service`.
- Risk: a client feature that repeats the outcomes of `CallAsync` grows with every operation and tests nothing new.
- Fix: a client feature checks only its own request and mapping.

# Check list
- [ ] Code delivery follows [Distinguish delivered code from illustrations](#distinguish-delivered-code-from-illustrations).
- [ ] `Client.Abstractions` and `Client.Http` are copies of the filled templates.
- [ ] Each API has `{ApiName}.Client.Abstractions` with `I{ApiName}Client` and `{ApiName}.Client` with `Prefix`, `Operations`, one method per operation, and `Mock/{ApiName}Mock.cs`; the contract scenario passes.
- [ ] `{Module}.Application` declares the consumer interface without client references; `{ApiServiceName}.Service` references `{ApiName}.Client.Abstractions`; the service maps `ApiUnavailableException`, the expected `ApiStatusException` codes, and nothing else to kinds.
- [ ] `Program.cs` loads the settings and registers the client and the service in `// [services]`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's startup command planning with another service's todo API: `make run` against its emulator, or against the running HTTP example of [vp-http-api-in-dotnet](library/architecture/dotnet/vp-http-api-in-dotnet.skill/vp-http-api-in-dotnet.skill.md).

- [Client implementation](./examples/src/Client/Todo/Todo.Client/TodoClient.cs) and [emulator](./examples/src/Client/Todo/Todo.Client/Mock/TodoMock.cs) — illustrate selected operations, wire-to-DTO mapping and mock behavior for the Todo provider.
- [Adapting service](./examples/src/Service/Todo/Todo.Service/RemotePlannerService.cs) and [service scenarios](./examples/src/Service/Todo/Todo.Service.Tests/features/remote-planner.feature) — illustrate DTO-to-Domain Model adaptation and application error meanings for the sample consumer.
