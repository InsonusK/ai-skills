---
name: solution-http-api-client
description: Skeleton — realizes SyncOutboundApi over HTTP (VP10). A typed HttpClient per external dependency, registered with resilience (timeout, retry, circuit breaker), exposed to the module as a narrow Shared interface that returns Result<T>, never raw HttpResponseMessage.
whenToUse: when a module must make synchronous request-response calls to another service over HTTP — defining the typed client, its resilience policy, and the Result-returning contract the handler consumes
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - api
  - http-client
  - resilience
  - solution/http-api-client
  - stack/dotnet
creates:
  - "App.Infrastructure.Clients.{Dependency}Client.cs"
  - "Shared.Clients.I{Dependency}Client.cs"
extends:
  - "Shared.csproj"
  - "App.Infrastructure.csproj"
  - "App.Host.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
---

> **Draft contract — no client yet.** VP10 has no v3 prior art. This skeleton fixes the shape (typed client in `App.Infrastructure`, narrow `I{Dependency}Client` contract in `Shared` returning `Result<T>`, resilience via the standard resilience handler). The retry/circuit-breaker defaults and the auth-token flow are finalized with the first real dependency.

# Goal
- Give a module one way to call an external HTTP service: an injected `I{Dependency}Client` (a `Shared` contract) whose methods return `Result<T>` — no `HttpClient`, no `HttpResponseMessage`, no status-code handling in the handler.
- Apply timeout, retry (idempotent verbs only), and circuit-breaker policy in one place per dependency.

# Core Principle
- `I{Dependency}Client` is a narrow `Shared` interface — one method per operation the module actually calls, each returning `Result<T>`.
- `{Dependency}Client` (in `App.Infrastructure`) is a typed `HttpClient` registered with `AddHttpClient<...>().AddStandardResilienceHandler()`; it maps transport/HTTP errors to `Result` and deserializes the body.
- The base address, timeouts, and auth come from configuration bound to a per-dependency options type.
- A non-2xx or a transport failure becomes a `Result.Error`/`Result.Unavailable`, not an exception — the handler branches on `Result`.

# Boundaries
- Inbound HTTP (this module's own API) is `solution-http-api-publication` (VP8) — unrelated.
- gRPC outbound is `solution-grpc-client` (VP11).
- The external service's contract (its OpenAPI/DTOs) is owned by that service; this solution consumes it, it does not define it.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj]] - hosts `Clients/{Dependency}Client.cs`

NUGET:
- `Microsoft.Extensions.Http.Resilience` {version} - `AddStandardResilienceHandler` (version in `Directory.Packages.props`)

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-client.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj]] - extend - add `Clients/{Dependency}Client.cs` + the `Shared` contract + `App.Host` registration

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-http-api-client.skill/Implementation/App.Infrastructure.csproj.extend.md#MUST|App.Infrastructure.csproj]]
- Expose the dependency to the module as `I{Dependency}Client` (a `Shared` contract) returning `Result<T>` — never `HttpClient` or `HttpResponseMessage`.
  - Risk: a handler holding an `HttpClient` owns retry, deserialization, and status handling, scattering transport concerns across the module.
  - Fix: the typed client in `App.Infrastructure` does all of that; the handler sees a `Result`.
- Register the client with a resilience handler; enable retry only for idempotent verbs.
  - Risk: no resilience → one slow dependency stalls every request; retrying a POST double-submits.
  - Fix: `AddStandardResilienceHandler`; retry GET/PUT/DELETE, not POST unless the endpoint is idempotent.

# Check list
- [ ] `I{Dependency}Client` in `Shared/Clients`, methods return `Result<T>`.
- [ ] `{Dependency}Client` in `App.Infrastructure/Clients`, typed `HttpClient` + resilience handler.
- [ ] Base address/timeout/auth from configuration.
- [ ] No `HttpClient`/`HttpResponseMessage` reaches a handler.
