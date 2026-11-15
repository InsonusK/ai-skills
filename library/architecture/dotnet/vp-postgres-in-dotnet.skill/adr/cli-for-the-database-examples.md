---
name: CLI for the database examples
description: How dotnet database examples expose their sample commands
problem: The HTTP and gRPC contracts of the database examples change only to exercise persistence and concurrency, expanding unrelated API scenarios and generated artifacts.
decision: Attach PostgreSQL to the CLI example; unit of work and row version inherit that chain. Go and Python keep HTTP until a catalog-wide decision.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The database examples need a way to call their sample commands. Maintaining
proto, swagger, both servers and their contract scenarios distracts from the
persistence behavior; adding CompleteItem required a scoped contract override.

# Selected variant
[[#CLI for the dotnet database chain]]

Chosen by the owner on 2026-11-12.

# Searched variants

## CLI for the dotnet database chain
**Selected.**

### Description
PostgreSQL attaches to `vp-cli-api-in-dotnet`. Its lists resource translates
flags to AddItem and ListItems. The unit of work example copies this base;
the row version example adds CompleteItem as another CLI action. Shared
persistence scenarios describe calls without naming an entry point.

### Benefits
- Every sample call runs locally with no server or generated API contract.
- Persistence scenarios exercise a separate process for each action.
- Concurrency adds one action without overriding unrelated contract scenarios.

### Costs
- The dotnet examples use a different entry point from Go and Python.
- The CLI adds a dependency to the example chain, although the persistence
  mechanism itself works with any entry point.

## Keep the HTTP and gRPC examples

### Description
Keep the proto example as the base, extend its contract for each database
sample, and scope contract assertions where the sample differs.

### Benefits
- All stacks demonstrate persistence over the same API.
- The examples also show a deployed service's request path.

### Costs
- Sample changes require regenerating contracts and maintaining two servers.
- API conformance and persistence demonstrations become coupled.
