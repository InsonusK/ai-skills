---
name: layer-map-and-owned-rules
description: Separate the dependency map from implementation contracts of its layers
problem: The app-layers skill loads API dispatch, command naming, orchestration and domain rules even when only the dependency map is needed.
decision: Keep topology and interface boundaries in app-layers; place implementation rules in app-api-layer, app-command-layer, app-domain-layer and the integration mechanism providing a service.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

Navigation amendment: [layer-contract-links](./layer-contract-links.md) replaces
the README-only discovery policy below; separation of rule ownership remains.

# Problem
The dependency map and layer implementation change for different reasons. Keeping
them together makes every topology review load command naming, entry dispatch and
testing details, while transport VPs have no focused shared API contract.

# Selected variant
[Layer map with owned implementation contracts](#layer-map-with-owned-implementation-contracts)

Owner decision, 2026-11-28.

# Searched variants

## Layer map with owned implementation contracts

**Selected.**

### Description
Keep responsibilities, allowed calls, logical sibling boundaries and interfaces
in app-layers. Move entry rules and their ADR into app-api-layer; command/factory
rules and their ADRs into app-command-layer; entity/domain-service rules into
app-domain-layer. API transport VPs consume the common API contract. Navigation
between finished contracts belongs in the develop README; the topology does not
link every detailed skill and therefore does not load them all. Historical
implementation references name the downstream decision without a loader link;
otherwise an old storage ADR would pull the full implementation catalog back
into the topology contract.

A service adapts the capability supplied by a concrete mechanism. That mechanism
owns the service implementation contract: api-client describes its integration
service, composition of clients, DTO conversion and tests. No app-service-layer
skill is introduced. This refines ownership of the API-specific service details
previously included alongside the sibling-boundary decision; allowed call
relationships and the own-store exception remain unchanged.

### Benefits
- A topology review loads the topology contract alone.
- Shared API requirements have one owner across HTTP, gRPC, CLI and jobs.
- Service implementation rules stay with the integration that requires them.

### Costs
- Existing references and ADR ownership must move together.
- Readers need README navigation to discover each implementation contract.

## Keep every rule in app-layers

### Description
Keep the layer table, naming, dispatch, mapping and testing requirements together.

### Benefits
- All rules are available in one file.

### Costs
- Topology loads unrelated implementation details and ownership is ambiguous.

## Separate skill for every layer including service

### Description
Create an implementation skill for every named layer immediately.

### Benefits
- A uniform layer-to-skill mapping.

### Costs
- A generic service skill duplicates integration-specific adaptation rules before
  a substantial independent implementation contract exists.
