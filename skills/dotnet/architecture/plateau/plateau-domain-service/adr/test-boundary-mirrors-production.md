---
name: test-boundary-mirrors-production
description: Derive plateau-domain-service test reference permissions from its production skills
problem: Narrow generated test lists contradict the production Allowed Dependencies they claim to mirror
decision: Link the assembled production boundary
tags:
  - plateau/domain-service
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The generated test skills say the production boundary is mirrored, then enumerate narrower permissions. Domain and Interfaces can depend on Shared and contracts; Application can use its permitted contracts. Restating a short test list makes those two rules disagree.

# Selected variant
[Link the assembled production boundary](#link-the-assembled-production-boundary)

# Searched variants

## Link the assembled production boundary
**Selected.**

### Description
Test projects reference their tested project and mirror its assembled Allowed Dependencies through a link to the production structural skill. Layer responsibilities still select which entry point a scenario proves. The existing Cecil extension separately contributes the assemblies its architecture scan inspects.

### Benefits
- Fulfils the owner's 2026-10-09 task without maintaining a second dependency list.
- Preserves production boundaries and existing architecture-test contributions.

### Costs
- Consumers load the linked production structural skill to derive reference permissions.

## Retain a narrower generated test list

### Description
Keep the old list limiting a test project to its tested assembly, even where production permits supporting dependencies.

### Benefits
- No generated test text changes.

### Costs
- Contradicts the catalog's exact-mirror rule and can block legitimate scenario fixtures.

# Propagation
The source correction belongs to solution-dotnet-conformance-testing. It is applied in plateau-core, plateau-domain-service and plateau-offline-sync-service through the parent chain. VP4 retains its shared-spec and Cecil scan contributions. Production code and example project-reference items are unchanged.
