---
name: service collection without generic host
description: How the dotnet base plateau builds its process — a plain ServiceCollection or the Generic Host
problem: The composition root needs a container for MediatR and the services; should it be the Generic Host (Host.CreateApplicationBuilder) with its configuration, logging, and lifetime, or a plain ServiceCollection?
decision: A plain ServiceCollection built in Program.RunAsync; settings only from environment variables; the entry points run by EntryRunner, each API variation point starting its own WebApplication inside its Entry.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Generic Host is the usual dotnet start: `appsettings.json` plus environment, logging providers, and `IHostedService` lifetimes. The base plateau already has one runner of the entry points, the same in every stack, and settings read from environment variables by the part that owns them.

# Selected variant
[[#Plain ServiceCollection]]
- Owner decision, 2026-10-21.

# Searched variants

## Plain ServiceCollection

**Selected.**

### Description
`new ServiceCollection()`, the three marked sections, `BuildServiceProvider()`, then `EntryRunner.RunAllAsync`. An HTTP, gRPC, or health variation point builds a `WebApplication` inside its own `Entry` and runs it until the token is cancelled.

### Benefits
- The same shape as the Go composition root: a variation point's instruction reads the same in both stacks.
- One lifecycle (EntryRunner) and one source of settings (the environment), as the devops guides of every stack expect.

### Costs
- No `appsettings.json` and no `IOptions` binding out of the box; a part reads its environment variables itself.
- Each web entry point has its own `WebApplication`, so a service registered there for ASP.NET is not in the root container unless passed in.

## Generic Host

### Description
`Host.CreateApplicationBuilder(args)`; entry points as `IHostedService`.

### Benefits
- Familiar; configuration from several sources, `IOptions`, built-in console logging.

### Costs
- A second lifecycle beside EntryRunner: a hosted service that fails does not stop the others the same way, and the exit code is the host's.
- Settings in two places (files and environment), unlike the other stacks.
