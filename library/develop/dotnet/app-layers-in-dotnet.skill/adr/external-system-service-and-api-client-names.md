---
name: external-system-service-and-api-client-names
description: Name client and adapting-service projects by their external responsibility
problem: A consuming module name does not identify the external system a service adapts, and a bare API project name hides its client role.
decision: Use ApiName.Client and ApiServiceName.Service independently of Module; suffix client abstraction and test projects consistently.
status: accepted
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Separate Client and Service projects need names that express external ownership
when one service composes several entity-focused API clients. The earlier Module
placeholder accidentally ties the adapting service to a consumer's module. A
provider can expose several APIs, so the service name cannot always be the name
of one client either.

# Selected variant
[[#External-system service and explicit client suffix]]

The owner selected this naming refinement. It preserves the project separation
and consumer port ownership of
[client-and-service-sibling-projects](./client-and-service-sibling-projects.md).

# Searched variants

## External-system service and explicit client suffix

**Selected.**

### Description
Use src/Service/{ApiServiceName}/{ApiServiceName}.Service for the adapting service
and src/Client/{ApiName}/{ApiName}.Client for each client, with adjacent
{ApiName}.Client.Abstractions and {Project}.Tests projects. ApiServiceName identifies
one external system or its coherent subsystem; ApiName identifies the selected
API/entity responsibility. Neither is the consuming Module. One-to-one names are
Todo.Service and Todo.Client; one-to-many names are Todo.Service, TodoTask.Client
and TodoLog.Client. A shared provider prefix makes ownership visible; explicit
constructor dependencies still determine ownership. Source namespaces retain the
application prefix plus Service.{ApiServiceName} or Client.{ApiName}, independently
of the assembly suffix. Consumer-owned interfaces stay in Application or Domain.

### Benefits
- Names identify both external responsibility and architectural role.
- One service can own several distinct clients without adopting one API's name.
- Consumer-module renames do not require renaming the external integration.

### Costs
- More explicit project names and longer project-reference paths.
- The shared provider prefix and actual ownership must remain consistent.

## Consumer-module service and bare API client

### Description
Keep {Module}.Service for the adapting implementation and {ApiName} for the
concrete client project, with {ApiName}.Abstractions for its public contract.

### Benefits
- Shorter client names and direct visual grouping with a module.
- No renaming of the original example projects.

### Costs
- The consuming module is confused with the external-system boundary.
- A bare API name does not show whether a project calls or serves that API.

## Use the same API name for client and service

### Description
Derive both {ApiName}.Client and {ApiName}.Service from one API name in every case.

### Benefits
- A simple pairing when exactly one client belongs to one service.

### Costs
- Several clients of one system cannot express a distinct system-level service.
- Splitting one API into multiple clients may force unnecessary service renames.
