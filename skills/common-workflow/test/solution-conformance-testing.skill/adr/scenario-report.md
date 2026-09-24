---
name: scenario-report
description: How the test report shows which behaviors are specified, of which type, and with which status — replacing the hand-maintained TESTS.md / test-trace-matrix.md that no-test-theater used to require
problem: no-test-theater required a hand-maintained trace matrix (TESTS.md / test-trace-matrix.md) per module listing every scenario, its type, the covering test, what it asserts, and its status. With every test case written as a Cucumber scenario, that matrix duplicates the .feature files and drifts from them; but the report produced by make test-report showed only pass/fail counts, coverage, and mutation score — not which kinds of behavior are covered.
decision: Classify every scenario with one type tag in the .feature file, have unit-test write a normalized tmp/result/scenarios.json (inventory from the .feature files, status from the runner), and have test-report render public/scenarios/index.html from it. No separate test inventory file.
tags:
  - solution/conformance-testing
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The trace matrix answered questions the report could not: which scenarios exist per feature, which type each one is (happy / boundary / negative / error / concurrency / security / regression), which are planned but not implemented and why, and — through a per-type summary — whether a whole kind of behavior is missing. Keeping that answer in a hand-written file next to the `.feature` files means every scenario is described twice, and the copy that is not executed goes stale first.

# Selected variant

**Selected variant:** [[#Normalized scenarios.json from tagged .feature files (selected)]]

# Searched variants

## Normalized scenarios.json from tagged .feature files (selected)

### Description

- The type is a Gherkin tag (`@happy`, `@negative`, …) — one per scenario or per `Examples:` block; a planned scenario is `@todo` with a `# todo:` reason comment. Both rules live in [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md|cucmber-testing]].
- `unit-test` builds the inventory by parsing the `.feature` files (so `@todo` entries appear) and joins each runner result onto it, writing `tmp/result/scenarios.json`.
- `test-report` renders `public/scenarios/index.html` from that file only.
- "What does it assert" is read from the `Then` data table in the `.feature`; "passes but asserts too little" from surviving mutants; "which changed branch is untested" from the coverage and mutation reports.

### Benefits

- One source of truth: the executed `.feature` file is the specification and the inventory.
- Stack-independent: the same JSON schema and page for go, .NET, Python, and TypeScript, matching how `unit-test.json`/`mutation-test.json` already work.
- A missing negative/error case, an untyped scenario, or a `@todo` without a reason is visible in the published report, not only at review time.

### Costs

- Each stack needs a small normalizer that parses `.feature` files and joins the runner's result by location or name.
- Scenario type is only as correct as the tag the author chose — the report makes the classification visible, it does not verify it.

## Keep TESTS.md / test-trace-matrix.md

### Description

Keep no-test-theater's hand-maintained matrix per module, updated in every PR that changes tests.

### Benefits

- No tooling; works for tests that are not Cucumber scenarios.

### Costs

- Duplicates the `.feature` files and drifts from them; nothing checks that the matrix matches what actually runs.
- Every test is a Cucumber scenario per [[skills/common-workflow/test/cucmber-testing.skill/cucmber-testing.skill.md|cucmber-testing]], so the "not a scenario" case the matrix covered no longer exists.

## Render the runner's own native report only

### Description

Rely on the runner's HTML report (`tmp/report/tests/`) and add nothing.

### Benefits

- No new file or code.

### Costs

- Native reports list only executed scenarios — `@todo` entries are absent — and have no notion of scenario type, so the per-type gap stays invisible.
- Each stack's native report looks different, so there is no one page to read across stacks.
