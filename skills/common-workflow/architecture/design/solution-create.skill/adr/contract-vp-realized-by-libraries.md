---
name: contract VP realized by libraries
description: How a Variation Point whose behaviour is fixed by a stack-agnostic contract (storage schema, lifecycle, conformance scenarios) is delivered to each stack
problem: A contract VP (TaskBox, Outbox, Inbox) needs hundreds of lines of concurrency-sensitive code per stack. Written as code inside a solution's Implementation files, it cannot be reviewed by reading, an agent applying it can silently drop a guarantee, and the rules drown in restated code — where should that code live, and what is left for the solution?
decision: One stack-agnostic spec repository per contract VP (the contract and its conformance feature) plus one library repository per stack that implements it and runs the pinned feature in its CI; the `-in-{stack}` solution only adds the library dependency and describes the seams between the library and the service.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`solution-taskbox-in-go` was built the usual way: every file of the mechanism was copied into `Implementation/`, into the plateau's `structure/`, and into the plateau example. Reviewing it, the owner found that (1) it could not be validated by reading, (2) nothing stops an agent from breaking a guarantee while copying 1,000 lines of concurrency code — the group lock and the outcome fence were exactly such lines — and (3) the solution's rules were lost among restated code. The mechanism already had library shape: it imported nothing of the service.

# Selected variant
[[#Spec repository plus one library repository per stack (selected)]]

# Searched variants

## Spec repository plus one library repository per stack (selected)

### Description
- `{vp-name}-spec` — stack-agnostic: the contract (schema versions, lifecycle, per-store realization) and the conformance feature with its step vocabulary, released by version tags.
- `{vp-name}-{stack}` (`-go`, `-dotnet`, `-python`, …) — the library: implements the contract, pins a spec version, runs its conformance feature against every store it supports in CI.
- In this repository: the VP concept (why and when) links the spec repository; the base `solution-{name}` keeps the stack-agnostic usage rules; `solution-{name}-in-{stack}` adds the dependency and describes only the seams (data port, transaction, handler adapter, composition root, migration).

### Benefits
- Validated by execution, not by reading: a library release is green conformance CI against the pinned spec.
- An agent never retypes the mechanism, so it cannot drop a guarantee; what it writes — the seams — is small and reviewable.
- Each stack's library has its own release cadence, CI, and owner (another agent can build one from a task file).
- Solutions become short and rule-dense again.

### Costs
- Four repositories per VP to maintain, each with its own releases.
- A stack library must fetch the feature from the spec repository at a pinned version (submodule or release download) — copying it would reintroduce drift.
- An agent working in a service needs network access to the spec repository for contract details; the VP concept must stay sufficient to decide whether the VP is needed.

## Code inside the solution's Implementation files

### Description
The status quo: the mechanism's source code, verbatim, in `Implementation/`, repeated in each plateau.

### Benefits
- Everything is in one repository; skills are self-contained.

### Costs
- The three problems above; verbatim copies need a mechanical equality check to stay honest.

## One repository per VP, all stacks inside

### Description
`taskbox/` with `spec/`, `go/`, `dotnet/`, … in one repository.

### Benefits
- The feature is shared by path, never pinned across repositories.

### Costs
- One repository mixes toolchains and release streams; a stack's release tags must be path-prefixed (`go/v0.1.0`), and a change in one stack's CI blocks the others.

## Libraries inside this skills repository (`libs/{stack}/…`)

### Description
Library code next to the skills.

### Benefits
- No new repositories.

### Costs
- A skills repository becomes a code monorepo with build toolchains; `ai-skill-manager sync` and library releases get entangled.

# Boundary
Only a VP whose behaviour is fixed by a stack-agnostic contract with conformance scenarios is delivered this way. A pattern VP that shapes the service's own code (ports and adapters, a persistent store adapter, an inbound API) stays a solution: packaged as a library it would become a framework dictating the service's structure.
