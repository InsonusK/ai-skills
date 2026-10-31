---
name: vp-http-client-in-dotnet
description: The HTTP client variation point in dotnet — Client.Abstractions (ApiUnavailableException, ApiStatusException, ApiStatusCode) and Client.Http (HttpApiConfig, HttpApiClient.CallAsync with the timeout, JSON in and out, the call log, the request id, the mock as an HttpMessageHandler) copied from templates, one client per API in Infrastructure/Client/{Api} with its {Api}.Abstractions, its mock, and its contract scenario, the service over it in {Module}.Application/Services, and the lines of Program.cs that build them
whenToUse: when a dotnet application built on the base plateau must call another service's HTTP API, when you call a further operation of such an API, or when you review a dotnet HTTP client
updated: 20261031
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
- **Copied once, written per operation** - `Client.Abstractions` and `Client.Http` are copied from templates and never edited; per API the application writes `{Api}.Abstractions` with its interface, `{Api}` with the client whose operations are calls of `HttpApiClient.CallAsync` and its mock, and the service over it.
- **The service sees outcomes, never a transport** - `{Module}.Application` references `{Api}.Abstractions`, which references `Client.Abstractions`: the service catches `ApiUnavailableException` and `ApiStatusException`, the same for an HTTP and a gRPC client. Decision recorded in [one-outcome-project-for-every-client](./adr/one-outcome-project-for-every-client.md).

```
docs/integration/client/{api-name}/openapi.json        the provider's contract, copied unchanged
src/Infrastructure/Client/                             every client of another service's API, and nothing else
├── Core/                                              copied from templates, never edited
│   ├── Client.Abstractions/   ApiOutcomes.cs          ApiUnavailableException, ApiStatusException, ApiStatusCode; no package
│   ├── Client.Http/           HttpApiConfig.cs        Load(prefix): {API}_HOST, _HTTP_PORT, _TLS, _TIMEOUT; Mock
│   │                          HttpApiClient.cs        CallAsync: timeout, JSON, X-Request-Id, call log, outcomes
│   └── Client.Http.Tests/                             the fixed code's scenarios, with a test API of their own
└── {Api}/                                             written per API (TodoApi)
    ├── {Api}.Abstractions/    I{Api}.cs               the interface in the API's terms, its own item types
    ├── {Api}/                 {Api}Client.cs          Prefix, Operations, one method per operation with its own types
    │                          Mock/{Api}Mock.cs       an HttpMessageHandler answering every operation ({API}_HOST=mock)
    └── {Api}.Tests/                                   each operation against a server of the scenario + the contract scenario
src/Modules/{Module}/{Module}.Application/Services/    I{Service}.cs, {Service}Service.cs, {Service}Errors.cs: outcomes → kinds
```

# Rule

## MUST

### Copy the client code from templates
Copy [Client.Abstractions](./templates/src/Infrastructure/Client/Core/Client.Abstractions/), [Client.Pipeline](./templates/src/Infrastructure/Client/Core/Client.Pipeline/), and [Client.Http](./templates/src/Infrastructure/Client/Core/Client.Http/) to `src/Infrastructure/Client/Core/`, with the example's `Client.Pipeline.Tests` and `Client.Http.Tests`, and add the five projects to the solution.
- Risk: a retyped client drifts in its timeout, its outcomes, or its log from every other dotnet client of the catalog.
- Fix: copy the files unchanged; `make test` passes.

### One client per API, behind its abstractions
Write each API as two projects under `src/Infrastructure/Client/{Api}/`: `{Api}.Abstractions` with `I{Api}`, one method per operation in the API's terms and its own item records, referencing only `Client.Abstractions`; and `{Api}` with `{Api}Client : I{Api}` holding `Prefix` (`"TODO_API"`), `Operations` (an `HttpOperation` - method, path, and `Retryable` where a repeat does what one call does - for every operation it calls), the client middleware in its constructor, and per operation one method that calls `HttpApiClient.CallAsync` with private request and response records and maps the answer.

```csharp
public async Task<IReadOnlyList<TodoApiItem>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
{
    var answer = await _http.CallAsync<PlanItemsResponse>(HttpMethod.Post, "/v1/items/plan", new PlanItemsRequest(titles), cancellationToken);
    return answer.Items.Select(item => new TodoApiItem(item.Title, item.Done)).ToList();
}
```

- Violation: `HttpClient` used in the client; `{Module}.Application` referencing `{Api}` itself; the client throwing an `ErrorKindException`; an operation missing from `Operations`.
- Risk: a call that bypasses `CallAsync` has no timeout, no log, and no request id; a service on the client's class cannot be tested with a fake.
- Fix: follow the example's [TodoApiClient.cs](./examples/src/Infrastructure/Client/TodoApi/TodoApi/TodoApiClient.cs); a new operation adds its method to `I{Api}` and `{Api}Client`, its line of `Operations`, and its route in [TodoApiMock.cs](./examples/src/Infrastructure/Client/TodoApi/TodoApi/Mock/TodoApiMock.cs).

### Answer every operation in the mock
Write the mock of each client as `src/Infrastructure/Client/{Api}/{Api}/Mock/{Api}Mock.cs`, an `HttpMessageHandler` that answers every operation of `Operations` as the API does, with stable data and no state, and pass it to `HttpApiClient` in the client; `{API}_HOST=mock` runs the client against it. The file's name says it is a mock and of which client.
- Risk: an operation the mock does not route answers 404 under `mock`, and the service cannot run without the other service.
- Fix: the mocked scenario of the client's feature runs it and checks its answer.

### Map the outcomes in the service
Write the service over the client in `{Module}.Application/Services/` with its interface beside it, per [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md), and its errors as factory methods of `{Service}Errors.cs` beside it: catch `ApiUnavailableException` as the kind unavailable, each `ApiStatusException` code it expects as the kind that fits, with the outcome as the inner exception, and let anything else pass with no kind; throw an entity's rule error raised by the API's answer as an exception with no inner exception.

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
- Fix: the scenario "Any other answer is a failure with no kind" of the example's `RemotePlanner.feature`.

### Build the client and the service in Program.cs
Load the settings with `HttpApiConfig.Load({Api}Client.Prefix)` in `// [services]` of `Program.cs`, stopping the start with "startup failed" on a `FormatException`, and register the client as a singleton of `I{Api}` built with a logger of the logger factory, and the service as a singleton of its interface. Give every client the client middleware of its API, per [api-client](library/develop/core/api-client.skill/api-client.skill.md): build the list in `// [client middleware]` inside the client's registration - empty until a variation point (`vp-client-retry`) adds one - and pass it to the client's constructor.

```csharp
services.AddSingleton<ITodoApi>(provider =>
{
    var loggers = provider.GetRequiredService<ILoggerFactory>();
    // [client middleware]
    IReadOnlyList<IClientMiddleware> todoApiMiddlewares = [];
    return new TodoApiClient(todoApiConfig, loggers.CreateLogger("Client.TodoApi"), todoApiMiddlewares);
});
services.AddSingleton<IRemotePlanner, RemotePlannerService>();
```

- Risk: settings read when the client is first resolved fail the first command instead of the start; a client built in a handler cannot be mocked or configured from one place.
- Fix: the handler takes the service's interface; the composition root builds both.

### Cover the client with scenarios
Copy the example's `Client.Http.Tests` unchanged: it covers the settings, the call, the outcomes, the log, and the mock through a test API of its own on `HttpListener`. Write per client an `{Api}.Tests` calling each operation against an `HttpListener` of the scenario and its mock, with the contract scenario reading the copied `openapi.json` linked into its output; per service and per command scenarios with the layer below faked by hand; tag them `@type/tech-check` and `@type/service`.
- Risk: a client feature that repeats the outcomes of `CallAsync` grows with every operation and tests nothing new.
- Fix: a client feature checks only its own request and mapping.

# Check list
- [ ] `Client.Abstractions` and `Client.Http` are unchanged copies of the templates.
- [ ] Each API has `{Api}.Abstractions` with `I{Api}` and `{Api}` with `Prefix`, `Operations`, one method per operation, and `Mock/{Api}Mock.cs`; the contract scenario passes.
- [ ] `{Module}.Application` references `{Api}.Abstractions` only; the service maps `ApiUnavailableException`, the expected `ApiStatusException` codes, and nothing else to kinds.
- [ ] `Program.cs` loads the settings and registers the client and the service in `// [services]`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's startup command planning with another service's todo API: `make run` against its emulator, or against the running HTTP example of [vp-http-api-in-dotnet](library/architecture/dotnet/vp-http-api-in-dotnet.skill/vp-http-api-in-dotnet.skill.md).
