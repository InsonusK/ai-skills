---
name: shared contract and example operations
description: How the common HTTP contract scenarios remain reusable when a sample adds operations
problem: A shared scenario that pins the base sample's routes rejects a valid application extending its contract.
decision: The common scenario compares served operations with OpenAPI; each example pins its own expected operations in a native feature.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The proto HTTP conformance scenario listed the Todo base operations. A sample
adding CompleteItem could serve exactly its extended contract and still fail
that scenario. Overriding its assertion locally hid the meaning of the copied
table and required a special case in every stack.

# Selected variant
[[#Shared equality and native operation pins]]

Chosen by the owner on 2026-11-14.

# Searched variants

## Shared equality and native operation pins
**Selected.**

### Description
The shared scenario checks served operations against OpenAPI without naming
sample operations. Every consuming example has operation-list-in-{stack}.feature
with a static table of its intended operations, checked against both routes and
OpenAPI. Adding a sample operation changes its own pin only.

### Benefits
- One unchanged shared scenario covers base and extended applications.
- Static example tables still catch accidentally missing or extra operations.
- No scoped assertion override is needed.

### Costs
- Every example must carry a native operation-list feature.
- Contract bindings support the shared comparison and the example assertion.

## Preserve the common pin and override it locally

### Description
Keep the original table in the shared copy and make extended samples override
its assertion by scenario scope, comparing their actual routes and contract.

### Benefits
- Existing shared copies and base bindings stay unchanged.

### Costs
- The copied table describes a different application from the one tested.
- Each stack needs an override and an additional operation assertion.
