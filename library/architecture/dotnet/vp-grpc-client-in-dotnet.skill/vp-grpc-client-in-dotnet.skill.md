---
name: vp-grpc-client-in-dotnet
description: The gRPC client variation point in dotnet — Client.Abstractions (the outcomes every client reports) and Client.Grpc (GrpcApiConfig, GrpcApiClient with a lazy GrpcChannel, CallInterceptor for the timeout, the x-request-id metadata, the call log, and the outcomes, EmulatorCallInvoker for the mock) copied from templates, the consumed proto compiled by Grpc.Tools into the client as internal types, one client per API in Infrastructure/Client/{Api} with its {Api}.Abstractions, its mock, and its contract scenario, the service over it in {Module}.Application/Services, and the lines of Program.cs that build them
whenToUse: when a dotnet application built on the base plateau must call another service's gRPC API, when you call a further method of such an API or replace its proto, or when you review a dotnet gRPC client
updated: 20261031
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The gRPC client variation point in dotnet** - A dotnet application that keeps every property of [vp-grpc-client](library/architecture/spec/vp-grpc-client.skill/vp-grpc-client.skill.md) and [api-client](library/develop/core/api-client.skill/api-client.skill.md), in the projects of [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md); this skill details their implementation for dotnet and restates none of their rules.
- **Attached by the listed changes only** - Attaching it adds projects and files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example.

# Core Principle
- **Copied once, compiled per contract, written per method** - `Client.Abstractions` and `Client.Grpc` are copied from templates and never edited; per API the application copies its `.proto`, compiles it in the client's project, and writes `{Api}.Abstractions`, the client whose methods are calls of the generated client over `GrpcApiClient.CallInvoker`, its mock, and the service over it.
- **The service is the HTTP client's** - The service catches only the outcomes of `Client.Abstractions`, the same for an HTTP and a gRPC client ([one-outcome-project-for-every-client](library/architecture/dotnet/vp-http-client-in-dotnet.skill/adr/one-outcome-project-for-every-client.md)): the service, its interface, its errors, and its scenarios are those of [vp-http-client-in-dotnet](library/architecture/dotnet/vp-http-client-in-dotnet.skill/vp-http-client-in-dotnet.skill.md), file for file.

```
docs/integration/client/{api-name}/{name}.proto       the provider's contract, copied unchanged
src/Infrastructure/Client/                             every client of another service's API, and nothing else
├── Core/                                              copied from templates, never edited
│   ├── Client.Abstractions/   ApiOutcomes.cs          ApiUnavailableException, ApiStatusException, ApiStatusCode; no package
│   ├── Client.Grpc/           GrpcApiConfig.cs        Load(prefix): {API}_HOST, _GRPC_PORT, _TLS, _TIMEOUT; Mock
│   │                          GrpcApiClient.cs        a lazy GrpcChannel, or the emulator, behind CallInterceptor
│   │                          CallInterceptor.cs      deadline, x-request-id, two records, status → outcome
│   │                          EmulatorCallInvoker.cs  the mock's transport: the generated service inside the process
│   └── Client.Grpc.Tests/                             the fixed code's scenarios, with an echo API of their own on Kestrel
└── {Api}/                                             written per API (TodoApi)
    ├── {Api}.Abstractions/    I{Api}.cs               the interface in the API's terms, its own item types
    ├── {Api}/                 {Api}.csproj            <Protobuf GrpcServices="Both" Access="Internal">
    │                          {Api}Client.cs          Prefix, Methods, one method per method mapping to its own types
    │                          Mock/{Api}Mock.cs       the generated service base implemented ({API}_HOST=mock)
    └── {Api}.Tests/                                   each method against a Kestrel server of the scenario + the contract scenario
```

# Rule

## MUST

### Copy the client code from templates
Copy [Client.Abstractions](./templates/src/Infrastructure/Client/Core/Client.Abstractions/), [Client.Pipeline](./templates/src/Infrastructure/Client/Core/Client.Pipeline/), and [Client.Grpc](./templates/src/Infrastructure/Client/Core/Client.Grpc/) to `src/Infrastructure/Client/Core/`, with the example's `Client.Pipeline.Tests` and `Client.Grpc.Tests`, add the five projects to the solution, and `Grpc.Net.Client`, `Google.Protobuf`, `Grpc.Tools`, and, for the scenarios, `Grpc.AspNetCore` to `Directory.Packages.props`. With the HTTP client attached too, `Client.Abstractions` and `Client.Pipeline` are already there, unchanged.
- Risk: a retyped client drifts in its deadline, its outcomes, or its log from every other dotnet client of the catalog.
- Fix: copy the files unchanged; `make test` passes.

### Compile the consumed contract into the client, internal
Compile the copied `.proto` in `{Api}.csproj` with `<Protobuf Include="..\..\..\..\..\docs\integration\client\{api-name}\{file}.proto" GrpcServices="Both" Access="Internal" />`, with `Google.Protobuf` and `Grpc.Tools`, and give its tests `InternalsVisibleTo`.
- Violation: the provider's generated package referenced; `Access="Public"`, which lets the service see the messages; the copy edited to change `csharp_namespace`.
- Risk: the service depends on the provider's messages, and a new version of the contract edits the module.
- Fix: the generated types live in the provider's namespace (`Contracts.Todo.V1`) inside the client's assembly only; nothing generated is committed, as [grpc-tools-at-build-time](library/architecture/dotnet/vp-grpc-api-in-dotnet.skill/adr/grpc-tools-at-build-time.md) decided for the API.

### One client per API, behind its abstractions
Write each API as `{Api}.Abstractions` with `I{Api}`, referencing only `Client.Abstractions`, and `{Api}` with `{Api}Client : I{Api}, IDisposable` holding `Prefix` (`"TODO_API"`), `Methods` (a `GrpcMethod` - the full name and `Retryable` where a repeat does what one call does - for every method it calls), a `GrpcApiClient` built with its mock's `Bind`, `Methods`, and the client middleware of its constructor, the generated client over its `CallInvoker`, and per method one method that calls it and maps the response.

```csharp
public async Task<IReadOnlyList<TodoApiItem>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
{
    var answer = await _items.PlanItemsAsync(new PlanItemsRequest { Titles = { titles } }, cancellationToken: cancellationToken);
    return answer.Items.Select(item => new TodoApiItem(item.Title, item.Done)).ToList();
}
```

- Violation: `GrpcChannel.ForAddress` in the client; a blocking or streaming call; a method missing from `Methods`.
- Risk: a channel that bypasses `GrpcApiClient` has no deadline, no log, and no request id; the contract scenario misses the method.
- Fix: follow the example's [TodoApiClient.cs](./examples/src/Infrastructure/Client/TodoApi/TodoApi/TodoApiClient.cs); a new method adds its method to `I{Api}` and `{Api}Client`, its line of `Methods`, and its override in [TodoApiMock.cs](./examples/src/Infrastructure/Client/TodoApi/TodoApi/Mock/TodoApiMock.cs).

### Answer every method in the mock
Write the mock of each client as `src/Infrastructure/Client/{Api}/{Api}/Mock/{Api}Mock.cs`, an internal class overriding the generated service base for every method of `Methods`, with stable data and no state, throwing `RpcException` where the API answers a status, and a static `Bind(ServiceBinderBase)` that binds it; `{API}_HOST=mock` serves it through `EmulatorCallInvoker` behind the same `CallInterceptor`, each message through its wire form.
- Risk: a method the mock does not override answers `UNIMPLEMENTED` under `mock`, and the service cannot run without the other service.
- Fix: the mocked scenario of the client's feature runs it and checks its answer.

### Build the client and the service in Program.cs
Load the settings with `GrpcApiConfig.Load({Api}Client.Prefix)` in `// [services]` of `Program.cs`, stopping the start with "startup failed" on a `FormatException`, and register the client as a singleton of `I{Api}` built with a logger of the logger factory, and the service as a singleton of its interface, as [vp-http-client-in-dotnet](library/architecture/dotnet/vp-http-client-in-dotnet.skill/vp-http-client-in-dotnet.skill.md) does; the provider disposes the client, and with it the channel. Build the client middleware of the API in `// [client middleware]` inside the client's registration - empty until a variation point (`vp-client-retry`) adds one - and pass it to the client's constructor.
- Risk: a channel built per call opens a connection per call; one never disposed holds its connection until the process ends.
- Fix: one singleton per API.

### Cover the client with scenarios
Copy the example's `Client.Grpc.Tests` unchanged: it covers the settings, the call, the outcomes, the log, and the mock through an echo API of its own, served on Kestrel over HTTP/2 in plain text. Write per client an `{Api}.Tests` calling each method against a Kestrel server of the scenario and its mock, and its contract scenario finding each of `Methods` in the descriptor compiled from the copied file (`{File}Reflection.Descriptor`); take the service's and the command's scenarios from the HTTP client; tag them `@type/tech-check` and `@type/service`.
- Risk: a client feature that repeats the outcomes of the interceptor grows with every method and tests nothing new.
- Fix: a client feature checks only its own request and mapping; the generated code cannot be stale, so no scenario compares it with the copy.

# Check list
- [ ] `Client.Abstractions` and `Client.Grpc` are unchanged copies of the templates.
- [ ] The consumed `.proto` is unchanged under `docs/integration/client/{api-name}/` and compiled in `{Api}.csproj` with `Access="Internal"`.
- [ ] Each API has `{Api}.Abstractions` with `I{Api}` and `{Api}` with `Prefix`, `Methods`, one method per method, and `Mock/{Api}Mock.cs`; the contract scenario passes.
- [ ] The service, its errors, and its scenarios are those of the HTTP client.
- [ ] `Program.cs` loads the settings and registers the client and the service in `// [services]`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's startup command planning with another service's todo API: `make run` against its emulator, or against the running gRPC example of [vp-grpc-api-in-dotnet](library/architecture/dotnet/vp-grpc-api-in-dotnet.skill/vp-grpc-api-in-dotnet.skill.md).
