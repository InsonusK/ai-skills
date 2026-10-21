---
name: grpc tools at build time
description: How a dotnet service gets the C# of its proto contract — Grpc.Tools at build time, or buf with the generated code committed as in Go
problem: The Go gRPC variation point generates the code with buf and commits it; should a dotnet service do the same, or let Grpc.Tools generate it during every build?
decision: Grpc.Tools compiles the proto file under docs/integration/api/ at build time (<Protobuf GrpcServices="Both">); nothing generated is committed; buf is not used. A service class overrides its generated base, and the contract scenario stands in for Go's compile-time check of a missing method.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go has no build step that runs protoc, so the Go catalog pins buf and its plugins as `tool` lines and commits `gen/`. MSBuild runs code generators as part of the build, and `Grpc.AspNetCore` brings `Grpc.Tools` for exactly that.

# Selected variant
[[#Grpc.Tools at build time]]
- Chosen with B3 (2026-10-21) as the stack's default; no fork for the owner, recorded so the departure from Go is explained.

# Searched variants

## Grpc.Tools at build time

**Selected.**

### Description
`<Protobuf Include="..\..\..\docs\integration\api\todo\todo.proto" GrpcServices="Both" />` in `App.Host.csproj`; the generated classes live in `obj/`.

### Benefits
- One source of the contract; generated code cannot go stale or be edited.
- No tool beside the SDK; the version of the generator is the package version in `Directory.Packages.props`.

### Costs
- The generated code is not in the repository to read; an IDE shows it after a build.
- The generated base's methods are virtual: an rpc not overridden compiles and answers UNIMPLEMENTED — so the contract scenario checks every rpc is overridden.

## buf with committed code, as in Go

### Description
`buf generate` into `gen/`, committed, a `make proto-gen` target.

### Benefits
- The same workflow as the Go catalog; the generated code is reviewable in a diff.

### Costs
- A second toolchain in a dotnet repository and a step every developer must remember; nothing stops a stale `gen/`.
