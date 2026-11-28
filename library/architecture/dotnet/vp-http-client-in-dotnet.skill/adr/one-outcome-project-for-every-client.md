---
name: one outcome project for every client
description: Where a dotnet service finds the technical outcomes of a client - one Client.Abstractions for HTTP and gRPC, or an abstractions project per transport as Go's packages
problem: An adapting service in {ApiServiceName}.Service references the logical {ApiName}.Client.Abstractions contract, so the outcomes it catches (unavailable, a status) must live in one; should every transport have its own, as Go's httpclient and grpcclient packages, or should one project serve both?
decision: One project, src/Client/Core/Client.Abstractions, holds ApiUnavailableException, ApiStatusException, and ApiStatusCode (google.rpc.Code, which is also the gRPC status code) for every client; each API's {ApiName}.Client.Abstractions references it, so the service over a client catches the same types whichever transport the client uses.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The earlier Go examples placed outcomes in transport packages; the current Go catalog uses a shared outcome package. In dotnet the service's project may reference only abstractions without packages of a technique (app-layers-in-dotnet), so the outcome types need an abstractions project of their own.

# Selected variant
[[#One Client.Abstractions]]
- Chosen by the owner (2026-10-26).

# Searched variants

## One Client.Abstractions

**Selected.**

### Description
`Client.Abstractions` (no package) beside `Client.Http` and `Client.Grpc` under `Client/Core/`; `ApiStatusException` carries the google.rpc code and message, with HTTP/gRPC diagnostics retained as an inner exception.

### Benefits
- The service over a client is the same for HTTP and gRPC; moving an API from one transport to the other changes the client only.
- google.rpc.Code and the gRPC status codes are one numbering, so one enum names both.

### Costs
- At selection time this differed from Go's transport-specific outcome packages.
- The common status has no HTTP field; transport diagnostics are retained only as the inner exception.

## An abstractions project per transport

### Description
`Client.Http.Abstractions` and `Client.Grpc.Abstractions`, each with its own exceptions, as Go's packages.

### Benefits
- The same shape as the Go catalog.

### Costs
- Two more projects and the same types twice; a service changes its catches and its project reference when its API changes transport.
