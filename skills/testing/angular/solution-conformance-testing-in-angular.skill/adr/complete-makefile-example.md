---
name: complete-makefile-example
description: What the skill ships as its ground truth - a complete Angular application behind the Makefile, or separate spec illustrations
problem: The solution's deliverable is a Makefile that hides the runners from CI and produces one report; spec snippets alone do not show that deliverable working
decision: One runnable Angular application in example/ that runs all four kinds and assembles the report through make
tags:
  - solution/conformance-testing-in-angular
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
  - framework/angular
---

# Problem
The solution delivers a `Makefile` contract: a caller runs `make` targets, knows no runner, and gets one report. Illustrations of a component spec and a browser spec do not show that contract working, nor that the inherited Cucumber and mutation kinds still run beside the two Angular ones.

# Selected variant
**Selected variant:** [[#Complete Angular application behind the inherited Makefile]]

# Searched variants

## Complete Angular application behind the inherited Makefile

**Selected.**

### Description
`example/` holds the Link checker domain and its scenarios unchanged from the TypeScript example, an Angular form that uses it, component and browser specs, a reviewed screenshot baseline, the lockfile, the four kind scripts and the unchanged shared report tools.

### Benefits
- `make init && make test-and-report` is the whole public path; CI needs no runner command.
- Every badge has real evidence behind it, and a failure report can be inspected.

### Costs
- More dependencies and a browser runtime in the repository's examples.
- The screenshot baseline assumes a controlled Linux and Chromium environment.

## Separate spec illustrations

### Description
Keep single spec snippets in the skill and verify the whole outside the repository.

### Benefits
- A smaller source footprint.

### Costs
- A reader cannot reproduce the report from the skill; the integration with the inherited Cucumber and mutation kinds stays unproved.
