---
name: entity-clients-and-system-services
description: Client grouping and service ownership across provider contracts
problem: How should a large provider contract be split, and may one external-system service use several clients?
decision: Group clients by entity API responsibility, allow partial contracts, and let one system service own several clients with explicit same-system sharing exceptions.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
How should a large provider contract be split, and may one external-system service use several clients?

# Selected variant
[Entity clients under a system service](#entity-clients-under-a-system-service)

Owner decision, 2026-11-26.

# Searched variants

## Entity clients under a system service

**Selected.**

### Description
A client covers the needed operations of one entity or cohesive resource, including reads; a large OpenAPI or proto file may have several clients. The copied provider file remains complete. Service adapts one external system, using several clients where needed. A large system can be decomposed into cohesive services. Each client normally has one owning service; sharing is exceptional, justified, and limited to those decomposed services of the same system. This replaces the one-client-per-API and one-service-per-client cardinality; it does not permit one service to integrate unrelated systems.

### Benefits
- Boundaries follow integration responsibilities rather than provider packaging.
- Several API clients can support one stable consumer-facing service.

### Costs
- Ownership and any sharing exception require explicit review.
- A provider endpoint used by several clients needs a consistent configuration policy.

## One file one client one service

### Description
Create a client and a service for each provider file.

### Benefits
- Boundaries are simple to discover.

### Costs
- Monolithic contracts create giant clients and fragment system adaptation.

## Clients shared without an owner

### Description
Let every service directly use any client.

### Benefits
- Maximum immediate reuse.

### Costs
- Adaptation duplicates and API changes have no clear owner.
