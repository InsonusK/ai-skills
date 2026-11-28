---
name: app-layers
description: The application layer map — responsibilities, sibling boundaries, allowed call directions and interfaces; implementation rules belong to each layer's own skill
whenToUse: when defining or reviewing application layer boundaries, directory ownership, allowed dependencies or interface placement
updated: 20261128
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/service-wraps-one-infrastructure.md
  - adr/sibling-client-and-system-service-boundaries.md
  - adr/layer-map-and-owned-rules.md
---

# Goal
- Every application element has a clear layer and permitted dependency direction.
- I/O boundaries can be replaced through contracts without reversing dependencies.

# Core Principle
- **Topology ownership** - Keep the layer map here and implementation requirements in their owning layer skills, as decided in [layer-map-and-owned-rules](./adr/layer-map-and-owned-rules.md).
- **Logical peers** - Directory siblings do not imply permission to call each other; the dependency table determines allowed calls.

# Scope
- Covers the stack-independent layer map; stack extensions determine physical packages and projects.
- Client/service paths in existing stack examples still await the planned propagation wave.

# Rule

## MUST

### Keep each part in its layer and its calls downward
Place every part of the application in one of these layers and let it call only what its row allows.

| Layer | Owns | May call | Never calls |
|---|---|---|---|
| api | parsing an input into plain parameters; turning a result or an error into the response | command factories, commands | domain, services, clients, infrastructure |
| command factory (optional) | choosing the command from plain parameters and the application's state | domain services, services | clients, infrastructure |
| command | the main logic of one operation | domain services, entities, services, the repositories of its own store (entities form), other commands | clients, infrastructure, api types |
| domain service | business logic reused by several commands | entities, services, the repositories of its own store (entities form) | clients, infrastructure, commands |
| entity | the rules of one object of the domain | other entities | anything with I/O |
| service | adaptation of a provided technical capability into consumer models | clients or its own infrastructure; entity construction/behavior | other services, domain services, commands |
| repository (entities form) | the entities of the application's own store, behind a contract of the application's side: pick them by a specification, save them | its store | every layer above |
| client | API calls, wire parsing, API DTOs, technical outcomes and client middleware | its external API, fixed client code | services, commands, domain |
| infrastructure | storage and other non-client technical access | its external system or engine | every layer above |

@import "./diagrams/layer-calls.mmd" {as="mermaid"}

Arrows show permitted calls, not directory containment; dotted arrows cross interfaces, including the own-store repository exception.

- Violation: a command that runs an SQL query, or a service that calls another service to finish its method.
- Risk: a call skipping a layer spreads the knowledge of that layer's technique upward, so replacing it means editing every caller.
- Fix: route the call through the layer in between; when two services must cooperate, a domain service or the command calls both. In the entities form, a command or domain service reaches its own store through the repository contract rather than a service. Decision recorded in [service-wraps-one-infrastructure](./adr/service-wraps-one-infrastructure.md).

### Keep client and service beside infrastructure
Keep `domain/`, `infrastructure/`, `client/` and `service/` as sibling logical boundaries, expressed through the stack's packages or projects.
- Violation: client code under `infrastructure/client/`; service code classified as an infrastructure subfolder.
- Risk: the folder structure conceals a distinct API contract boundary and permits transport concerns to spread upward.
- Fix: follow the separation recorded in [sibling-client-and-system-service-boundaries](./adr/sibling-client-and-system-service-boundaries.md); retain the own-store repository exception in the layer table.

### Interfaces at service client and infrastructure boundaries
Make every caller of a service, and every service calling its clients or infrastructure, depend on an interface; call entities and domain services directly.
- Violation: a command holding a concrete `UserHTTPClient`, or an interface written for an entity.
- Risk: without the interface, the layer above cannot be tested without the real system or replaced at startup; an interface over code with no I/O only adds a file.
- Fix: the interface lists the methods the caller uses; the composition root passes the implementation.

## MAY

### Pure helpers usable from any layer
Use a package with no I/O and no business rule — formatting, layout, conversion — from any layer above infrastructure.

# Check list
- [ ] Check the [dependency table and diagram](#keep-each-part-in-its-layer-and-its-calls-downward).
- [ ] Check [sibling boundaries](#keep-client-and-service-beside-infrastructure).
- [ ] Check [interface boundaries](#interfaces-at-service-client-and-infrastructure-boundaries).
