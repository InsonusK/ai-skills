---
name: testing tool choice
description: Which Gherkin runner, coverage tool, and mutation-testing tool the TypeScript conformance-testing solution uses
problem: Pick one Gherkin/BDD runner, one coverage tool, and one mutation-testing tool for framework-agnostic TypeScript packages that must satisfy the solution-conformance-testing gate
decision: "@cucumber/cucumber + c8 + Stryker (@stryker-mutator/core)"
tags:
  - solution/conformance-testing-in-typescript
  - concern/documentation
  - concern/documentation/adr
  - stack/typescript
---

# Problem
[solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md) requires a Gherkin runner, a coverage tool, and a mutation-testing tool, but leaves the concrete choice to each stack. Framework-agnostic TypeScript packages need one specific, documented choice so every package applying this solution uses the same tools, independent of whatever UI framework eventually consumes the package.

The first choice named Vitest for coverage and for unit tests beside the scenarios. It was replaced on 2026-10-08: the unit kind never ran Vitest, and the owner's rule for front-end code is that tests of services and classes are Cucumber scenarios, while component and pixel tests are test kinds of the UI framework's own testing solution.

# Selected variant
**Selected variant:** [[#cucumber-js c8 Stryker]]

# Searched variants

## cucumber-js c8 Stryker

**Selected.**

### Description
Use `@cucumber/cucumber` (the official JS/TS Cucumber implementation) for every test of the package, `c8` for the coverage of that run, and Stryker (`@stryker-mutator/core`) with its `command` test runner calling `cucumber-js` for mutation testing.

### Benefits
- One runner: the unit kind, the coverage number and mutation testing all see the same tests.
- `@cucumber/cucumber` is the reference implementation of Cucumber for JavaScript/TypeScript; it writes Cucumber Messages, the standard report the living doc is rendered from.
- `c8` wraps any Node process, so coverage needs no second runner and no instrumentation step.
- Stryker Mutator is the same tool family as Stryker.NET in `solution-conformance-testing-in-dotnet`, keeping the mutation report's shape consistent across stacks.

### Costs
- Stryker's `command` runner starts `cucumber-js` once per mutant and cannot tell which test covers which mutant: slow on a large package, and no mutant is ever reported as `NoCoverage`.
- A test that is awkward as a scenario has no other home in this package.

## cucumber-js Vitest coverage Stryker

### Description
Use Vitest for unit tests and its built-in coverage, with `@cucumber/cucumber` beside it for the scenarios.

### Benefits
- Vitest is the runner most TypeScript packages already use; its coverage needs no extra tool.

### Costs
- Two runners in one test kind: two exit codes and two coverage outputs to merge, and mutation testing must run both.
- It duplicates what the scenarios already prove for a package of rules and validators.

## jest-cucumber instead of @cucumber/cucumber

### Description
Express Gherkin scenarios as Jest test functions via `jest-cucumber` instead of running `@cucumber/cucumber` as a separate process.

### Benefits
- Scenarios run inside the existing test runner process, one less CLI invocation in CI.

### Costs
- Requires Jest specifically, and gives up `@cucumber/cucumber`'s Cucumber Messages report the living doc is rendered from.

## Playwright's own BDD-style fixtures instead of Cucumber

### Description
Use Playwright's test fixtures with a BDD-flavored naming convention instead of real Gherkin `.feature` files.

### Benefits
- One fewer tool/dependency.

### Costs
- Not actually Gherkin — loses the [solution-conformance-testing](skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md) goal of one readable spec format shared across stacks, and cannot be reused as-is by a non-TypeScript implementation of the same rule.
