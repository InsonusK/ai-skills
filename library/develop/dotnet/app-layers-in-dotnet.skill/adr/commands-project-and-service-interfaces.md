---
name: commands project and service interfaces
description: What a dotnet module shows outside itself, and where the interfaces to its services and to the infrastructure are declared
problem: The module's command records lived in a project named {Module}.Interfaces, which read as the place for service interfaces; a service interface put there would be visible to every entry point; and the first service variation point needs a place for the service, its interface, and the infrastructure's interface.
decision: The module's public project is {Module}.Commands (commands and their results only), seen by entry points and other modules; another module is called only by its commands; a service's interface is declared beside its consumer ({Module}.Application/Services, or Domain for a domain service) with its implementation in Application; each infrastructure is two projects, {Infra}.Abstractions and {Infra}, shared by modules.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
B1 named the module's public project `{Module}.Interfaces`; it held only commands, but the name invited service interfaces into it. Discussed with the owner on 2026-10-21 before the HTTP form of a proto contract, whose example is the first to keep data (an in-memory store) and so the first with a service and an infrastructure.

# Selected variant
[[#Commands project, service interfaces in Application, infrastructure in two projects]]
- Owner decisions, 2026-10-21: a module calls another by its commands, so a module needs no interface project of its own; a service interface stays in Application for DI and for replacing the implementation; the project of commands is `{Module}.Commands`; the infrastructure's interface project is `{Infra}.Abstractions`.

# Searched variants

## Commands project, service interfaces in Application, infrastructure in two projects

**Selected.**

### Description
```
App.Host/Api, Other.Application ──► Todo.Commands       commands + results
Todo.Application  handlers, Services/IItemLists + ItemListsService ──► Todo.Domain
                                     └──► Memory.Abstractions (IListStore) ◄── Memory (MemoryListStore)
```

### Benefits
- An entry point or another module sees commands and nothing else, so every call goes through the pipeline behaviors.
- The service interface lists only what the module's handlers call, as app-layers' consumer-owned interface; the composition root swaps the implementation, the scenarios fake it.
- A module's `Application` never sees a package of the technique (Npgsql, EF Core): it references only `{Infra}.Abstractions`.

### Costs
- A service in `Application` can technically send a command or use a handler of its module; only the rule forbids it.
- Two projects per infrastructure.
- B1–B3's examples renamed `Todo.Interfaces` to `Todo.Commands`.

## Service interfaces in {Module}.Interfaces, commands moved to {Module}.Commands

### Description
`{Module}.Interfaces` keeps the service interfaces as the module's public API; commands in their own project.

### Benefits
- Other modules can call a service of the module directly.

### Costs
- A call between modules past the commands skips their behaviors and binds the modules' insides; with modules talking by commands, the project has no consumer outside the module.

## Interface declared by its provider

### Description
`Todo.Services` declares `IItemLists` beside its class; `Application` references it.

### Benefits
- Every reference points strictly downward.

### Costs
- The interface grows to every method any caller uses; the consumer sees the implementation class and can construct it.
