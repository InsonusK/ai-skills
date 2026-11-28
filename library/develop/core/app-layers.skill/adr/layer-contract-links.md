---
name: layer-contract-links
description: Discover implementation contracts directly from the layer map
problem: Readers cannot open a layer implementation contract directly from the dependency table when discovery is confined to README.
decision: Link the existing shared implementation contracts from the layer table and accept their transitive loading, while retaining implementation rules in their owners.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The layer map identifies responsibilities and permitted calls but requires another
navigation page to find their implementation contracts. Direct links improve
navigation, while the skill loader also follows those links into implementation
skills and their dependencies.

# Selected variant
[Implementation links in the layer table](#implementation-links-in-the-layer-table)

Owner request, 2026-11-28. This amends discovery in
[layer-map-and-owned-rules](./layer-map-and-owned-rules.md), preserving its rule
ownership and the decision against a standalone service-layer skill.

# Searched variants

## Implementation links in the layer table

**Selected.**

### Description
Add a contract column linking the shared API, command, domain and API-client
skills. Service refers to its providing mechanism, with api-client specifically
identified as the owner of API adaptation. Repository and infrastructure rows
name concrete VP ownership without inventing a generic skill or prescribing one
storage engine. Keep detailed rules in their owning skills.

### Benefits
- A reader reaches the appropriate contract directly from the layer table.
- Missing common contracts and mechanism-specific ownership are explicit.

### Costs
- Loading app-layers also loads the linked implementation contracts and their
  transitive dependencies; the layer map is no longer a minimal load boundary.
- Common contracts link back to app-layers, so dependency traversal has cycles.

## README-only discovery

### Description
Keep app-layers free of outgoing implementation links and navigate through README.

### Benefits
- Loading the dependency map does not pull implementation rules.

### Costs
- Finding the contract for a table row requires another navigation step.
