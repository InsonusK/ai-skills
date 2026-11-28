---
name: vp-grpc-client-in-dotnet
description: The gRPC client variation point in dotnet — Client.Abstractions (the outcomes every client reports) and Client.Grpc (GrpcApiConfig, GrpcApiClient with a lazy GrpcChannel, CallInterceptor for the timeout, the x-request-id metadata, the call log, and the outcomes, EmulatorCallInvoker for the mock) copied from templates, the consumed proto compiled by Grpc.Tools into the client as internal types, entity-focused clients in Client/{ApiName} with its {ApiName}.Client.Abstractions, its mock, and its contract scenario, the adapting implementation in Service/{ApiServiceName}/{ApiServiceName}.Service, and the lines of Program.cs that build them
whenToUse: when a dotnet application built on the base plateau must call another service's gRPC API, when you call a further method of such an API or replace its proto, or when you review a dotnet gRPC client
updated: 20261128
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
- **Copied once, compiled per contract, written per method** - `Client.Abstractions` and `Client.Grpc` are copied from templates and never edited; per API the application copies its `.proto`, compiles it in the client's project, and writes `{ApiName}.Client.Abstractions`, the client whose methods are calls of the generated client over `GrpcApiClient.CallInvoker`, its mock, and the service over it.
- **Protocol-independent adaptation** - Service depends on logical API DTOs and `Client.Abstractions` outcomes, preserving its implementation when an equivalent HTTP/gRPC client is exchanged, as [one-outcome-project-for-every-client](library/architecture/dotnet/vp-http-client-in-dotnet.skill/adr/one-outcome-project-for-every-client.md) records.

```text
docs/integration/client/{api-name}/       provider's wire contract
src/Client/
  Core/{Client.Abstractions,Client.Pipeline,Client.Grpc}/
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
Copy [Client.Abstractions](./templates/src/Client/Core/Client.Abstractions/), [Client.Pipeline](./templates/src/Client/Core/Client.Pipeline/), and [Client.Grpc](./templates/src/Client/Core/Client.Grpc/) to `src/Client/Core/`, with the example's `Client.Pipeline.Tests` and `Client.Grpc.Tests`, add the five projects to the solution, and `Grpc.Net.Client`, `Google.Protobuf`, `Grpc.Tools`, and, for the scenarios, `Grpc.AspNetCore` to `Directory.Packages.props`. With the HTTP client attached too, `Client.Abstractions` and `Client.Pipeline` are already there, unchanged.
- Risk: a retyped client drifts in its deadline, its outcomes, or its log from every other dotnet client of the catalog.
- Fix: copy the files unchanged after namespace substitution; `make test` passes.

### Compile the consumed contract into the client, internal
Compile the copied `.proto` in `{ApiName}.Client.csproj` with `<Protobuf Include="..\..\..\..\docs\integration\client\{api-name}\{file}.proto" GrpcServices="Both" Access="Internal" />`, with `Google.Protobuf` and `Grpc.Tools`, and give its tests `InternalsVisibleTo`.
- Violation: the provider's generated package referenced; `Access="Public"`, which lets the service see the messages; the copied wire contract changed while qualifying its local namespace.
- Risk: the service depends on the provider's messages, and a new version of the contract edits the module.
- Fix: qualify only the local `csharp_namespace` as `{service-namespace}.Contracts.{Name}.V{major}`, preserving protobuf packages, service names and fields; generated types stay internal to the client assembly and are never committed, as [grpc-tools-at-build-time](library/architecture/dotnet/vp-grpc-api-in-dotnet.skill/adr/grpc-tools-at-build-time.md) decided for the API.

### Entity-focused client contracts and implementations
Write each selected entity-focused API client as `{ApiName}.Client.Abstractions` with `I{ApiName}Client`, referencing only `Client.Abstractions`, and `{ApiName}.Client` with `{ApiName}Client : I{ApiName}Client, IDisposable` holding `Prefix` (the selected `{API}` settings group), `Methods` (a `GrpcMethod` - the full name and `Retryable` where a repeat does what one call does - for every method it calls), a `GrpcApiClient` built with its mock's `Bind`, `Methods`, and the client middleware of its constructor, the generated client over its `CallInvoker`, and per method one method that calls it and maps the response.

```csharp
public async Task<IReadOnlyList<TodoItemDto>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
{
    var answer = await _items.PlanItemsAsync(new PlanItemsRequest { Titles = { titles } }, cancellationToken: cancellationToken);
    return answer.Items.Select(item => new TodoItemDto(item.Title, item.Done)).ToList();
}
```

- Violation: `GrpcChannel.ForAddress` in the client; a blocking or streaming call; a method missing from `Methods`.
- Risk: a channel that bypasses `GrpcApiClient` has no deadline, no log, and no request id; the contract scenario misses the method.
- Fix: add each consumed method to `I{ApiName}Client`, implement it in `{ApiName}Client` through the fixed transport code, register it in `Methods`, and implement the corresponding mock behavior.

### Answer every method in the mock
Write the mock of each client as `src/Client/{ApiName}/{ApiName}.Client/Mock/{ApiName}Mock.cs`, an internal class overriding the generated service base for every method of `Methods`, with stable data and no state, throwing `RpcException` where the API answers a status, and a static `Bind(ServiceBinderBase)` that binds it; `{API}_HOST=mock` serves it through `EmulatorCallInvoker` behind the same `CallInterceptor`, each message through its wire form.
- Risk: a method the mock does not override answers `UNIMPLEMENTED` under `mock`, and the service cannot run without the other service.
- Fix: the mocked scenario of the client's feature runs it and checks its answer.

### Implement the adapting service for its consumer
Write the adapting implementation in `src/Service/{ApiServiceName}/{ApiServiceName}.Service/` against its consumer-owned interface and the logical client contracts, adapting operations and API DTOs to Domain Model through the domain's public APIs.
- Violation: returning client DTOs to the handler; catching `RpcException` in service; treating invalid provider data as invalid caller input.
- Risk: transport or provider changes reach consumers, or a broken dependency response is attributed to the caller.
- Fix: translate `ApiUnavailableException` to unavailable and expected `ApiStatusException` codes to application meanings, let other technical failures pass through without a kind, construct Domain Model through its public APIs, and keep the service interface beside its consumer.

### Build the client and the service in Program.cs
Load the settings with `GrpcApiConfig.Load({ApiName}Client.Prefix)` in `// [services]` of `Program.cs`, stopping the start with "startup failed" on a `FormatException`, and register the client as a singleton of `I{ApiName}Client` built with a logger of the logger factory, and the service as a singleton of its interface; the provider disposes the client, and with it the channel. Build the client middleware of the API in `// [client middleware]` inside the client's registration - empty until a variation point (`vp-client-retry`) adds one - and pass it to the client's constructor.
- Risk: a channel built per call opens a connection per call; one never disposed holds its connection until the process ends.
- Fix: one singleton per API.

### Cover the client with scenarios
Copy the example's `Client.Grpc.Tests` with the chosen service namespace: it covers the settings, the call, the outcomes, the log, and the mock through an echo API of its own, served on Kestrel over HTTP/2 in plain text. Write per client an `{ApiName}.Client.Tests` calling each method against a Kestrel server of the scenario and its mock, and its contract scenario finding each of `Methods` in the descriptor compiled from the copied file (`{File}Reflection.Descriptor`); test each adapting service in `{ApiServiceName}.Service.Tests` with client contracts faked and each command with its service port faked; tag them `@type/tech-check` and `@type/service`.
- Risk: a client feature that repeats the outcomes of the interceptor grows with every method and tests nothing new.
- Fix: check client request/wire mapping separately from service DTO-to-Domain Model mapping and application error meanings, faking the boundary below each subject; build-time-generated code needs no stale-copy comparison.

# Check list
- [ ] Code delivery follows [Distinguish delivered code from illustrations](#distinguish-delivered-code-from-illustrations).
- [ ] `Client.Abstractions` and `Client.Grpc` are copies of the filled templates.
- [ ] The consumed `.proto` and generated access follow [Compile the consumed contract into the client, internal](#compile-the-consumed-contract-into-the-client-internal).
- [ ] Each API has `{ApiName}.Client.Abstractions` with `I{ApiName}Client` and `{ApiName}.Client` with `Prefix`, `Methods`, one method per method, and `Mock/{ApiName}Mock.cs`; the contract scenario passes.
- [ ] Service follows [Implement the adapting service for its consumer](#implement-the-adapting-service-for-its-consumer); its tests exercise DTO mapping and outcome translation through fake clients.
- [ ] `Program.cs` loads the settings and registers the client and the service in `// [services]`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's startup command planning with another service's todo API: `make run` against its emulator, or against the running gRPC example of [vp-grpc-api-in-dotnet](library/architecture/dotnet/vp-grpc-api-in-dotnet.skill/vp-grpc-api-in-dotnet.skill.md).

- [RemotePlannerService.cs](./examples/src/Service/Todo/Todo.Service/RemotePlannerService.cs) and [RemotePlannerErrors.cs](./examples/src/Service/Todo/Todo.Service/RemotePlannerErrors.cs) — application-specific adaptation and error translation, written per integration rather than copied as fixed templates.
- [IRemotePlanner.cs](./examples/src/Modules/Todo/Todo.Application/Services/IRemotePlanner.cs) — the consumer-owned service port.
- [remote-planner.feature](./examples/src/Service/Todo/Todo.Service.Tests/features/remote-planner.feature) and [RemotePlannerSteps.cs](./examples/src/Service/Todo/Todo.Service.Tests/Steps/RemotePlannerSteps.cs) — service behavior exercised with fake clients.

- [Client implementation](./examples/src/Client/Todo/Todo.Client/TodoClient.cs) and [emulator](./examples/src/Client/Todo/Todo.Client/Mock/TodoMock.cs) — illustrate selected operations, wire-to-DTO mapping and mock behavior for the Todo provider.
