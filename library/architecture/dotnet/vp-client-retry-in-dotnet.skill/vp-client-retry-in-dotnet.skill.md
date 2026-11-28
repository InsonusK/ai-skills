---
name: vp-client-retry-in-dotnet
description: The client retry variation point in dotnet — the Retry project in src/Client/Middleware copied from templates (RetryConfig.Load reading {API}_RETRY_ATTEMPTS and {API}_RETRY_BACKOFF, RetryMiddleware repeating a retryable call on ApiUnavailableException with doubled, jittered waits that the caller's cancellation ends, and a WARN record per repeat), listed in the client middleware of an API in Program.cs
whenToUse: when a dotnet application with a client of another service's API (vp-http-client-in-dotnet, vp-grpc-client-in-dotnet) must repeat calls the API did not answer, when you mark an operation retryable, or when you review a dotnet client retry
updated: 20261128
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The client retry variation point in dotnet** - A dotnet application that keeps every property of [vp-client-retry](library/architecture/spec/vp-client-retry.skill/vp-client-retry.skill.md); this skill details its implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds the `Retry` project and changes only the files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-http-client-in-dotnet](library/architecture/dotnet/vp-http-client-in-dotnet.skill/vp-http-client-in-dotnet.skill.md)'s example; over [vp-grpc-client-in-dotnet](library/architecture/dotnet/vp-grpc-client-in-dotnet.skill/vp-grpc-client-in-dotnet.skill.md) it is attached the same way.

# Core Principle
- **An IClientMiddleware of Client.Pipeline** - `RetryMiddleware` sees the `ClientCall` (API, operation, `Retryable`) and calls `next`, the attempt, once or more; `HttpApiClient` and `CallInterceptor` run it around their attempts, so it is the same over HTTP and gRPC.
- **The caller's cancellation ends the wait** - A `CancellationToken` carries no deadline to check before a wait, so `Task.Delay` takes the caller's token and a cancelled wait throws the `ApiUnavailableException` of the last attempt.

```
src/Client/
├── Core/Client.Pipeline/          of the clients: ClientCall, ClientAttempt, IClientMiddleware, ClientPipeline
├── Middleware/Retry/              copied from templates, never edited
│   ├── RetryConfig.cs             Load(prefix): {API}_RETRY_ATTEMPTS (3), {API}_RETRY_BACKOFF (100ms)
│   └── RetryMiddleware.cs         Retryable + ApiUnavailableException → WARN, wait, attempt again
├── Middleware/Retry.Tests/        the scenarios, with attempts that end as each scenario lists
└── {ApiName}/{ApiName}.Client/{ApiName}Client.cs     Operations / Methods: Retryable where a repeat does what one call does
```

# Rule

## MUST

### Distinguish delivered code from illustrations
Fill and copy explicitly delivered templates and fixed support files as instructed, and write application-specific client/service code for the selected API and consumer, treating snippets and application-specific files under `examples/` as illustrations.
- Violation: adopting an example's provider names, operations, DTOs or error meanings without deriving them from the target application's contracts.
- Risk: code compiles against the sample API while implementing the wrong application behavior.
- Fix: substitute only declared identifiers in delivered fixed code; derive application-specific operations, configuration prefixes, DTOs, Domain Model mapping and error meanings from the target contracts.

### Fill the service namespace
Fill `{service-namespace}` with the application's namespace root from [the namespace convention](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md#qualify-namespaces-with-the-service-name) before copying this skill's templates.
- Risk: an unfilled identifier does not compile, while a different root in each file splits the service's types.
- Fix: substitute the same identifier in every delivered source, preserve template logic, and verify no `{service-namespace}` remains in application sources.

### Copy the middleware from templates
Copy [Retry](./templates/src/Client/Middleware/Retry/) to `src/Client/Middleware/Retry/`, with the example's `Retry.Tests`, add both projects to the solution, and reference `Retry` from `App.Host`.
- Risk: a retyped retry repeats unmarked calls or statuses, or waits without jitter.
- Fix: copy the files unchanged after namespace substitution; `make test` passes.

### List the retry in the client middleware of the API
Load `RetryConfig.Load({ApiName}Client.Prefix)` beside the client's settings in `// [services]` of `Program.cs`, in the same `try` that stops the start on a `FormatException`, and add a `RetryMiddleware` with a logger of the logger factory to the list of `// [client middleware]` in the adapter's registration.

```csharp
IReadOnlyList<IClientMiddleware> todoApiMiddlewares =
[
    new RetryMiddleware(todoApiRetry, loggers.CreateLogger("Client.Retry")),
];
```

- Violation: the retry around an adapter method or a service; one `RetryConfig` shared by two APIs.
- Risk: a repeat outside the client repeats the mapping and misses the mark; two APIs cannot be tuned apart.
- Fix: one list per API, in its registration; a middleware after the retry runs once per attempt.

### Mark the retryable operations in the adapter
Set `Retryable: true` on each `HttpOperation` or `GrpcMethod` of the adapter's list whose repeat does what one call does, with a comment saying why, and hold the mark in the contract scenario's table.
- Risk: an unmarked operation is never repeated, a wrongly marked one is done twice.
- Fix: justify retry safety from the operation contract, set the retryable mark only when repetition is safe, and verify the selected marks in the contract scenarios.

# Check list
- [ ] Code delivery follows [Distinguish delivered code from illustrations](#distinguish-delivered-code-from-illustrations).
- [ ] `src/Client/Middleware/Retry` is an copy of the filled templates; `App.Host` references it.
- [ ] `Program.cs` loads the retry settings of each API and lists the middleware in its client middleware.
- [ ] Only operations whose repeat does what one call does are `Retryable`; the contract scenario holds the marks.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the HTTP client example with the retry attached: `TODO_API_HOST=localhost TODO_API_HTTP_PORT=1 make run` shows three attempts and two "api client call retried" warnings before the startup command fails unavailable.

- [Client implementation](./examples/src/Client/Todo/Todo.Client/TodoClient.cs) and [emulator](./examples/src/Client/Todo/Todo.Client/Mock/TodoMock.cs) — illustrate selected operations, wire-to-DTO mapping and mock behavior for the Todo provider.
- [Adapting service](./examples/src/Service/Todo/Todo.Service/RemotePlannerService.cs) and [service scenarios](./examples/src/Service/Todo/Todo.Service.Tests/features/remote-planner.feature) — illustrate DTO-to-Domain Model adaptation and application error meanings for the sample consumer.
