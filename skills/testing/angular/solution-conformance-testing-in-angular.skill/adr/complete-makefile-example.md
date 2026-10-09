---
name: complete-makefile-example
description: Require one runnable Angular application demonstrating the complete testing report contract.
problem: Loose component illustrations do not demonstrate the consumer-facing Makefile contract.
decision: Ship a complete Angular example using inherited domain tests and the two Angular kinds.
updated: 20261008
tags:
  - solution/conformance-testing-in-angular
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
  - framework/angular
---

# Problem
The solution's output is a Makefile that hides runner-specific details from CI and users and generates an inspectable testing report. Component snippets and a temporary proof omitted that deliverable from the repository.

# Selected variant
[Complete Angular application behind the inherited Makefile](#complete-angular-application-behind-the-inherited-makefile).

# Searched variants
## Complete Angular application behind the inherited Makefile
**Selected.**

Description: ship the real TypeScript Link checker domain/scenarios, an Angular form consuming it, native component/browser specs, a reviewed screenshot baseline, lockfile, all four kind adapters and unchanged shared report tools in `example/`.

Benefits: `make init && make test-and-report` is the complete public path; CI needs no runner commands; every badge has real evidence and failure reports can be inspected.

Costs: more dependencies and a browser runtime; the visual baseline assumes a controlled Linux/Chromium environment.

## Keep loose source illustrations and a temporary verification project
Description: preserve individual spec snippets and document verification outside the repository.

Benefits: smaller source footprint.

Costs: users cannot reproduce the full report from the skill's example; inherited Cucumber/mutation integration remains unproved.

# Consequences
The runnable example replaces loose illustrations as the source of truth. The shared core Makefile/report assembler remains unchanged; only the project-specific domain mutation selector is scoped. Component coverage remains distinct from inherited domain coverage. No dependent solution or plateau currently references this Angular extension.
