---
name: sibling-client-and-system-service-boundaries
description: Client as a separate logical boundary beside infrastructure
problem: Should external API clients remain under infrastructure, and does one service still mean exactly one infrastructure adapter?
decision: Use sibling domain infrastructure client and service boundaries; one service may adapt several clients of one external system.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Should external API clients remain under infrastructure, and does one service still mean exactly one infrastructure adapter?

# Selected variant
**Selected variant:** [[#Sibling client and service boundaries]]

Owner decision, 2026-11-26.

# Searched variants

## Sibling client and service boundaries

**Selected.**

### Description
Domain, infrastructure, client and service are sibling logical boundaries. Client handles API mechanics and DTOs, infrastructure retains non-client technical access, and service adapts one external system or a cohesive part of it. Physical package/project paths follow the stack. This amends [service-wraps-one-infrastructure](./service-wraps-one-infrastructure.md): API clients are no longer infrastructure children and a service may use several clients of one system. The own-store entities-form repository exception, domain/command orchestration across systems, and prohibition on service-to-service calls remain. Service may construct entities through domain APIs; client never references domain entities.

### Benefits
- API integration is visible in the top-level logical structure.
- System adaptation can compose several clients without exposing them to commands.

### Costs
- Stack layouts, client package paths and examples require migration.
- Directory peers alone do not enforce allowed dependency directions.

## Clients inside infrastructure

### Description
Keep clients under infrastructure and one service per adapter.

### Benefits
- No path migration.

### Costs
- The layout and cardinality conflict with the chosen integration boundary.

## Client is the consumer facing service

### Description
Merge API mechanics and domain adaptation into client.

### Benefits
- Fewer separate types.

### Costs
- Transport DTOs and consumer semantics share ownership.
