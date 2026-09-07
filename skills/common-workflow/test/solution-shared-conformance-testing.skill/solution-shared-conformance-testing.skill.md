---
name: solution-shared-conformance-testing
description: Defines how a group of Cucumber test cases that must hold identically across multiple components/implementations of the same system is extracted into its own shared spec project, reused by every component instead of re-authored per component
whenToUse: when the same functional guarantee must be proven identically by two or more components/implementations of a system, or when deciding whether a group of Cucumber scenarios should move out of one project into a shared, reusable spec project
domain: skill
type: architecture
version: 1
tags:
  - skill/architecture/solution
  - solution/component-conformance-testing
  - stack
  - concern/testing
  - concern/testing/bdd
  - cucumber

creates:
  - "{Component}.ConformanceSpec"
extends:
depends_on:
  - "[[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]"
built_on_plateau:
adr:
---

# Goal
- Create an approach where functionality that must be equivalent across different components of a system is covered by the same set of tests, instead of each component re-authoring its own copy of the same scenarios.

# Capabilities
- One `.feature` file, written once, proves the same functional guarantee against every component that must satisfy it.
- A component that starts failing a shared scenario is caught by that component's own test run, without anyone having to remember to keep two independent copies of the same scenario in sync.

# Core Principles
- Building on [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]], once a group of Cucumber test cases turns out to be needed by more than one component, that group is extracted out of the single project it started in.
- The extracted `.feature` files move into their own directory, which becomes its own project — owned by no single component, referenced by all of them.
- Each component's own test project references the shared spec project and supplies its own step definitions there, binding the shared scenarios to that component's real implementation.
- A scenario is extracted here only once it is genuinely needed by a second component — never speculatively, the same lazy-centralization discipline this catalog already applies elsewhere.
- The shared project holds only `.feature` files — no step definitions and no production code. Proving the scenarios (coverage, mutation testing) still happens inside each component's own project, per [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]].

# Requirements
SOLUTION:
- [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]
  - The shared scenarios are still proven the same way — Cucumber + coverage + mutation, inside each component's own project. This solution only changes where the `.feature` files live, not how they are proven.

# Template Skill Mutations
REPOSITORY:
- [[skills/common-workflow/test/solution-shared-conformance-testing.skill/Implementation/Repository.create|Repository]] - create - shared spec project holding only `.feature` files, no step definitions

# Rule

## MUST
- [[skills/common-workflow/test/solution-shared-conformance-testing.skill/Implementation/Repository.create#MUST|Repository]]
- Extract a scenario group into the shared spec project only once a second component genuinely needs it.
  - Risk: extracting speculatively produces a shared project with exactly one consumer — indirection with no reuse benefit yet.
  - Fix: keep the scenario inside the single component's own project until a real second consumer exists.
- Give every component consuming the shared spec its own step definitions bound to its own real implementation.
  - Risk: a shared step-definition implementation would prove only one component's code, defeating the reason the spec is shared in the first place.
  - Fix: keep step definitions local to each component; only the `.feature` files themselves are shared.
- Keep the shared spec project to `.feature` files only — never place step definitions or production code inside it.
  - Risk: the shared project stops being a pure, language/implementation-agnostic spec and becomes coupled to one component's tooling.

# Check list
- [ ] Every scenario in the shared spec project is consumed by two or more components.
- [ ] The shared spec project contains only `.feature` files — no step definitions, no production code.
- [ ] Each consuming component has its own step definitions binding the shared scenarios to its own real implementation.
