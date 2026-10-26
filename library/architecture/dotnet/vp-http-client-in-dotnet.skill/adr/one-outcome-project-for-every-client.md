---
name: one outcome project for every client
description: Where a dotnet service finds the technical outcomes of a client - one Client.Abstractions for HTTP and gRPC, or an abstractions project per transport as Go's packages
problem: A service in {Module}.Application may reference only an {Infra}.Abstractions project, so the outcomes it catches (unavailable, a status) must live in one; should every transport have its own, as Go's httpclient and grpcclient packages, or should one project serve both?
decision: One project, src/Infrastructure/Client/Core/Client.Abstractions, holds ApiUnavailableException, ApiStatusException, and ApiStatusCode (google.rpc.Code, which is also the gRPC status code) for every client; each API's {Api}.Abstractions references it, so the service over an adapter catches the same types whichever transport the adapter uses.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
In Go the service imports the client package of its transport and checks `httpclient.ErrUnavailable` or `grpcclient.ErrUnavailable`. In dotnet the service's project may reference only abstractions without packages of a technique (app-layers-in-dotnet), so the outcome types need an abstractions project of their own.

# Selected variant
[[#One Client.Abstractions]]
- Chosen by the owner (2026-10-26).

# Searched variants

## One Client.Abstractions

**Selected.**

### Description
`Client.Abstractions` (no package) beside `Client.Http` and `Client.Grpc` under `Infrastructure/Client/Core/`; `ApiStatusException` carries the google.rpc code, its message, and the HTTP status when there is one.

### Benefits
- The service over an adapter is the same for HTTP and gRPC; moving an API from one transport to the other changes the adapter only.
- google.rpc.Code and the gRPC status codes are one numbering, so one enum names both.

### Costs
- A departure from Go, where each transport's package has its outcomes.
- `HttpStatus` is null for every gRPC outcome.

## An abstractions project per transport

### Description
`Client.Http.Abstractions` and `Client.Grpc.Abstractions`, each with its own exceptions, as Go's packages.

### Benefits
- The same shape as the Go catalog.

### Costs
- Two more projects and the same types twice; a service changes its catches and its project reference when its API changes transport.
