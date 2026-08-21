---
name: mutation-tool-per-stack
description: Which mutation-testing tool this approach uses for each stack
problem: Mutation testing needs one concrete tool per stack. Leaving the choice unstated at this level invites each stack's own extending solution to pick independently, risking inconsistent tooling for stacks that are conceptually similar (e.g. C# and TypeScript both have mature Stryker support).
decision: Stryker for C#/.NET and for Angular/TypeScript. Mutmut for Python.
tags:
  - solution/conformance-testing
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Mutation testing (see the parent solution's Core Principles) needs a concrete tool to run. Different ecosystems have different mature options, and leaving the choice entirely to each stack's own extending solution risks two consumers of the same underlying tool family (e.g. a C# backend and an Angular frontend, both part of one system) picking differently for no real reason, or re-arguing a choice that is actually already settled across this catalog.

# Selected variant

**Selected variant:** [[#Stryker for .NET and Angular/TypeScript, Mutmut for Python (selected)]]

# Searched variants

## Stryker for .NET and Angular/TypeScript, Mutmut for Python (selected)

### Description

Standardize on Stryker Mutator for both C#/.NET (`dotnet-stryker`) and Angular/TypeScript (`@stryker-mutator/core`), since Stryker's core engine and reporting are shared across both. Python uses Mutmut, a separate, Python-native mutation-testing tool, since Stryker does not support Python.

### Benefits

- One mutation-testing tool family (Stryker) covers both .NET and Angular/TypeScript, so a full-stack system built from both gets one consistent report shape and one set of operational quirks to learn.
- Mutmut is a mature, actively maintained, Python-native tool — no need to force a JVM- or Node-based mutation tool onto a Python project.
- The decision is recorded once here instead of being made independently (and possibly inconsistently) by each stack's own extending solution.

### Costs

- Python's report shape/CLI differs from Stryker's, so the stack-agnostic parts of this approach (the `Makefile` targets, the report contract) must tolerate two different underlying tool behaviors rather than one.

## Let each stack's own extending solution choose independently

### Description

Do not record a tool choice here; each stack's own solution (e.g. a future `dotnet-solution-conformance-testing`-style extension) picks and records its own mutation-testing tool via its own ADR.

### Benefits

- Maximum flexibility — a stack can pick a better tool later without touching this solution.

### Costs

- No cross-stack consistency signal — two stacks that could reasonably share the same tool family (.NET and TypeScript, both via Stryker) might diverge for no real reason.
- The choice most likely to be genuinely stack-specific (Python needing a different tool than .NET/TypeScript) is exactly the kind of decision worth recording once, not re-deriving per stack.
