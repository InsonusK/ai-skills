---
name: success-status-by-outcome
description: Decide which HTTP status the HTTP form of a grpc method answers on success.
problem: The canonical gRPC-to-HTTP mapping answers 200 for every success, so a caller cannot tell created, accepted-for-later, and done apart. Which success statuses does the HTTP form use?
decision: 201 when a business object was created, 202 when the work was accepted and completes asynchronously, 200 otherwise.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
In one project, a method that only queued work answered 200 and callers treated the work as finished. The canonical mapping has no way to say "accepted, not done".

# Selected variant
**Selected variant:** [[#201, 202, or 200 by outcome]]
- HTTP callers get the distinction HTTP was designed to carry, at the cost of one per-method setting.

# Searched variants

## 201, 202, or 200 by outcome

**Selected.**

### Description
Each method declares one success status by what it did: 201 created a business object, 202 accepted asynchronous work, 200 anything else.

### Benefits
- A caller can tell created, accepted, and done apart from the status alone.
- Matches what HTTP clients and tooling expect from REST-style APIs.

### Costs
- Departs from the canonical mapping: a grpc-gateway server needs a forward-response hook to set the status per method.
- The status must be decided and documented per method.

## Always 200 (canonical mapping)

### Description
Every success answers 200, as grpc-gateway does by default.

### Benefits
- No per-method configuration; identical to the gateway default.

### Costs
- Queued work is indistinguishable from completed work; callers must read the body to know.
