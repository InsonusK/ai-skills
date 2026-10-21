---
name: vp-grpc-api-in-dotnet
description: The gRPC API variation point in dotnet — Api/Grpc/Pipeline copied from templates (settings, the server on its own HTTP/2 WebApplication, reflection behind a switch, the call log and the error answers as interceptors), one class per proto service in Api/Grpc/Endpoint overriding its generated base, the registration in Api/Grpc/Services.cs, the contract compiled by Grpc.Tools at build time, and one grpc Entry in Program.cs
whenToUse: when a dotnet application built on the base plateau must be served over gRPC, when you serve a new command over gRPC in such an application, or when you review its Api/Grpc folders
updated: 20261021
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
adr:
  - adr/grpc-tools-at-build-time.md
---

# Goal
- **The gRPC variation point in dotnet** - A dotnet application that keeps every property of [vp-grpc-api](library/architecture/spec/vp-grpc-api.skill/vp-grpc-api.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example.

# Core Principle
- **A method is written for its success** - A method maps its request message to the command, sends it, and maps the result to the response message; every exception it lets out is answered once, by the `ErrorAnswers` interceptor, through [RpcStatus](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md) — the same table as the HTTP variation point's.
- **Copied once, written per command** - The entry point is split as [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill.md#split-each-entry-point-into-pipeline-endpoint-and-registration) requires: `Pipeline/` is copied from templates and never edited; per command the application writes, in its service's class of `Endpoint/`, one override.

```
docs/integration/api/{contract-name}/{file}.proto    the contract, written by hand; C# generated at build time
src/App/App.Host/Api/
├── Errors/RpcStatus.cs        copied from error-kinds-in-dotnet; the same in every network entry point
├── Hosting/WebHost.cs         copied; the same in every web entry point (HTTP, management, gRPC)
└── Grpc/
    ├── Pipeline/              copied from templates, never edited
    │   ├── GrpcConfig.cs  GrpcServer.cs  GrpcService.cs
    │   ├── CallLog.cs         interceptor: received / answered, request id
    │   └── ErrorAnswers.cs    interceptor: every exception as its kind's status
    ├── Endpoint/              written per application, grows with the API
    │   └── ItemsHandler.cs    one class per proto service ≈ a controller
    └── Services.cs            GrpcEndpoints (one parameter per service), Services, GrpcApi.RunAsync
```

# Rule

## MUST

### Copy the entry point from templates
Copy, unchanged, [GrpcConfig.cs](./templates/src/App/App.Host/Api/Grpc/Pipeline/GrpcConfig.cs), [GrpcServer.cs](./templates/src/App/App.Host/Api/Grpc/Pipeline/GrpcServer.cs), [GrpcService.cs](./templates/src/App/App.Host/Api/Grpc/Pipeline/GrpcService.cs), [CallLog.cs](./templates/src/App/App.Host/Api/Grpc/Pipeline/CallLog.cs), and [ErrorAnswers.cs](./templates/src/App/App.Host/Api/Grpc/Pipeline/ErrorAnswers.cs) to `src/App/App.Host/Api/Grpc/Pipeline/`, [WebHost.cs](./templates/src/App/App.Host/Api/Hosting/WebHost.cs) to `Api/Hosting/`, and `RpcStatus.cs` of [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md) to `Api/Errors/`, unless another entry point already put the last two there.
- Risk: a retyped server drifts in its error answers, its call log, or its shutdown from every other dotnet service of the catalog; a second, different `WebHost.cs` breaks the HTTP or management entry point it is shared with.
- Fix: copy the files; `bash test/check-vp-attach.sh` in this repository fails when two variation points add a shared file differently.

### Write the contract and compile it with Grpc.Tools
Write `docs/integration/api/{contract-name}/{file}.proto` as [todo.proto](./examples/docs/integration/api/todo/todo.proto) shows — `// version:` first, `package {name}.v{major};`, `option csharp_namespace = "Contracts.{Name}.V{major}";` — per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md), and compile it in `App.Host.csproj` with `<Protobuf Include="..\..\..\docs\integration\api\{contract-name}\{file}.proto" GrpcServices="Both" Link="Api\Grpc\Contract\{file}.proto" />`, beside `Grpc.AspNetCore` and `Grpc.AspNetCore.Server.Reflection` (2.84.0 in `Directory.Packages.props`) and the `Microsoft.AspNetCore.App` framework reference. Decision recorded in [grpc-tools-at-build-time](./adr/grpc-tools-at-build-time.md).
- Violation: message types written by hand; generated `.cs` files committed or edited; buf in a dotnet repository; the proto file copied next to the code.
- Risk: the server and its clients disagree on the wire format; a committed generated file goes stale when the proto file changes.
- Fix: the build regenerates the code from the one proto file under `docs/integration/api/`; `GrpcServices="Both"` gives the scenarios the client they call the server with.

### One class per service in Endpoint
Write each service of the proto contract as `sealed class {Service}Handler(ISender sender) : {Service}.{Service}Base` in `Api/Grpc/Endpoint/{Service}Handler.cs`, with one `override` per rpc that maps the request message to the command (`using {Command}Command = ...;` when the rpc and the command share a name), sends it, and maps the result to the response message. Keep in `Api/Grpc/Services.cs` only `sealed record GrpcEndpoints({Service}Handler {Service}, ...)`, its `Services` (`GrpcService.Of({Service})` per service), and `GrpcApi.RunAsync`.
- Violation: a `try/catch` or a `throw new RpcException(...)` in a method; a generated message passed to a command or returned from one; a request that cannot become the command answered by anything but `throw new ErrorKindException(ErrorKind.Invalid, "...")`.
- Risk: a method answering its own errors answers differently from the table and from the HTTP form; a generated message in a command ties the command layer to the contract.
- Fix: follow the example's [ItemsHandler.cs](./examples/src/App/App.Host/Api/Grpc/Endpoint/ItemsHandler.cs) and [Services.cs](./examples/src/App/App.Host/Api/Grpc/Services.cs); an rpc kept but switched off throws `new ErrorKindException(ErrorKind.Unimplemented, "{Rpc} is deprecated since {version}")`, which the table answers UNIMPLEMENTED.

### Replace the startup entry in Program.cs
Replace the startup `Entry` in `// [entries]` of `Program.cs` with a `grpc` one calling `GrpcApi.RunAsync` with `GrpcEndpoints` built from `ISender`, and delete `Api/Startup/` with its feature and steps.

```csharp
// [entries] - every entry point, one Entry each, run together by EntryRunner.
var loggers = provider.GetRequiredService<ILoggerFactory>();
var grpcEndpoints = new GrpcEndpoints(Items: new ItemsHandler(sender));
var error = await EntryRunner.RunAllAsync(logger, stopping,
    new Entry("grpc", stopping => GrpcApi.RunAsync(grpcEndpoints, loggers, stopping)));
return EntryRunner.ExitCode(error);
```

- Violation: the settings loaded or the server built in `Program.cs`; `AddGrpc` in the root container; `GrpcApi.RunAsync` awaited outside `RunAllAsync`; the startup entry kept beside it.
- Risk: a server started outside the list is not stopped when another entry point fails; services registered in the root container are never seen by the gRPC server, which has a container of its own.
- Fix: `GrpcApi.RunAsync` reads the settings, builds the server on HTTP/2, and serves until `stopping` is cancelled; other entries of the list (health, HTTP) stay as they are — beside the HTTP entry point, the two `ItemsHandler` classes are told apart by their namespace.

### Cover the entry point with scenarios
Copy the example's `src/App/App.Host.Tests/features/Grpc/` scenarios of the fixed code (`Call`, `CallLog`, `GrpcConfig`, `Reflection`, `GrpcServe`) with their steps (`Steps/Grpc/GrpcWorld.cs`, `GrpcCallSteps`, `CallLogSteps`, `GrpcConfigSteps`, `ReflectionSteps`, `GrpcServeSteps`), the test service `Protos/test.proto`, `Steps/CapturedLogs.cs`, `Steps/FreePort.cs`, and `features/RpcStatus.feature` with its steps, and add `Microsoft.AspNetCore.TestHost`, `Grpc.Net.Client`, and `Grpc.Tools` to the test project; write one feature per service, calling it through the generated client with its handlers faked, and the contract scenario — every service of the proto file served, and every rpc overridden; tag them `@type/api/grpc` and `@type/tech-check`.
- Violation: a step text without "gRPC" in it; no scenario that every rpc is overridden.
- Risk: a step text two variation points both define makes Reqnroll fail as ambiguous once both are attached; an rpc added to the proto file and not overridden compiles — the generated base's method is virtual — and answers UNIMPLEMENTED in production.
- Fix: name the entry point in every step (`the gRPC call should succeed with ...`); the contract scenario finds a method left to the generated base; `make test` passes.

## SHOULD

### Serve a new command in its service's class
Serve a further command of an existing service by adding its rpc and messages to the proto file and its override to the service's class, and its scenario to the service's feature; a new service adds its class and feature, one parameter of `GrpcEndpoints`, one line of `Services`, and one row of the contract scenario.

# Check list
- [ ] `Api/Grpc/Pipeline/{GrpcConfig,GrpcServer,GrpcService,CallLog,ErrorAnswers}.cs`, `Api/Hosting/WebHost.cs`, and `Api/Errors/RpcStatus.cs` are copies of the templates.
- [ ] The proto file is versioned per api-develop under `docs/integration/api/`, compiled by `<Protobuf ... GrpcServices="Both">`; no generated file is committed.
- [ ] Each proto service is one `{Service}Handler` class in `Api/Grpc/Endpoint/` overriding every rpc with no error handling of its own; `Services.cs` holds only `GrpcEndpoints`, `Services`, and `GrpcApi.RunAsync`.
- [ ] `// [entries]` has a `grpc` entry calling `GrpcApi.RunAsync`; `Api/Startup/` is deleted.
- [ ] Every step text names its entry point; the contract scenario holds the services and their methods; `make test` passes.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the gRPC API attached: `make run`, `make test`.
