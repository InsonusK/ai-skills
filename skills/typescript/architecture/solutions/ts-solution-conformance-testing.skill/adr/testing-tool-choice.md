---
name: testing tool choice
description: Which Gherkin runner, coverage tool, and mutation-testing tool the TypeScript conformance-testing solution uses
problem: Pick one Gherkin/BDD runner, one coverage tool, and one mutation-testing tool for framework-agnostic TypeScript packages that must satisfy the solution-conformance-testing gate
decision: "@cucumber/cucumber + Vitest coverage + Stryker (@stryker-mutator/core)"
---

# Problem
[solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md) requires a Gherkin runner, a coverage tool, and a mutation-testing tool, but leaves the concrete choice to each stack. Framework-agnostic TypeScript packages need one specific, documented choice so every package applying this solution uses the same tools, independent of whatever UI framework eventually consumes the package.

# Selected variant
**Selected variant:** [[#cucumber-js Vitest coverage Stryker]]

# Searched variants

## cucumber-js Vitest coverage Stryker

### Description
Use `@cucumber/cucumber` (the official JS/TS Cucumber implementation) for Gherkin scenarios, Vitest's built-in coverage (`--coverage`, v8 provider) for coverage, and Stryker (`@stryker-mutator/core`) for mutation testing.

### Benefits
- `@cucumber/cucumber` is the reference implementation of Cucumber for JavaScript/TypeScript, with the widest ecosystem support.
- Vitest coverage requires no extra tooling beyond the test runner most modern TS packages already use.
- Stryker Mutator is the same vendor/tool family already selected for .NET in [[skills/dotnet/architecture/solutions/dotnet-solution-conformance-testing.skill/dotnet-solution-conformance-testing.skill|the dotnet variant]] (as Stryker.NET), keeping the mutation-report shape and mental model consistent across stacks.

### Costs
- `@cucumber/cucumber` runs as a separate process from Vitest, so combining their coverage output requires configuring both to write to the same coverage provider/output directory.
- Stryker's mutation runs are slow on large packages, which is why `make mutation-test` supports `ONLY_DELTA`/`DELTA_BASE` — see [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) for how the PR-gate workflow uses it.

## jest-cucumber instead of @cucumber/cucumber

### Description
Express Gherkin scenarios as Jest test functions via `jest-cucumber` instead of running `@cucumber/cucumber` as a separate process.

### Benefits
- Scenarios run inside the existing test runner process, one less CLI invocation in CI.

### Costs
- Requires Jest specifically; this solution's package already standardizes on Vitest for unit tests and coverage, and mixing two test runners in one package adds configuration surface for no functional gain.

## Playwright's own BDD-style fixtures instead of Cucumber

### Description
Use Playwright's test fixtures with a BDD-flavored naming convention instead of real Gherkin `.feature` files.

### Benefits
- One fewer tool/dependency.

### Costs
- Not actually Gherkin — loses the [solution-conformance-testing](skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md) goal of one readable spec format shared across stacks, and cannot be reused as-is by a non-TypeScript implementation of the same rule.
