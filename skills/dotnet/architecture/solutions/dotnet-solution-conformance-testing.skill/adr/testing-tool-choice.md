---
name: testing tool choice
description: Which Gherkin runner, coverage tool, and mutation-testing tool the .NET conformance-testing solution uses
problem: Pick one Gherkin/BDD runner, one coverage tool, and one mutation-testing tool for .NET projects that must satisfy the bdd-coverage-mutation-testing gate
decision: Reqnroll + coverlet/ReportGenerator + Stryker.NET
---

# Problem
[bdd-coverage-mutation-testing](skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md) requires a Gherkin runner, a coverage tool, and a mutation-testing tool, but leaves the concrete choice to each stack. .NET needs one specific, documented choice so every project applying this solution uses the same tools.

# Selected variant
**Selected variant:** [[#Reqnroll xUnit coverlet ReportGenerator Stryker.NET]]

# Searched variants

## Reqnroll xUnit coverlet ReportGenerator Stryker.NET

### Description
Use Reqnroll (the actively maintained successor to SpecFlow) with the xUnit runner for Gherkin scenarios, `coverlet.collector` + `ReportGenerator` for coverage, and Stryker.NET (`dotnet-stryker`) for mutation testing.

### Benefits
- Reqnroll is the actively maintained project; SpecFlow is in maintenance mode with contributors redirecting new work to Reqnroll.
- Stryker.NET is the de-facto standard .NET mutation-testing tool, with first-class GitHub Actions support and JSON/HTML report output that a badge can be generated from.
- coverlet + ReportGenerator is the standard combination already used for coverage on most modern .NET projects, avoiding a separate coverage engine from the test runner.

### Costs
- Reqnroll is a relatively young rename (2024); existing SpecFlow-based projects need a migration.
- Stryker.NET mutation runs are slow on large modules, which is why `make mutation-test` supports `ONLY_DELTA`/`DELTA_BASE` — see [devops-github-wf-bdd-report-publish](skills/devops/devops-github-wf-bdd-report-publish.skill/devops-github-wf-bdd-report-publish.skill.md) for how the PR-gate workflow uses it.

## SpecFlow xUnit coverlet Stryker.NET

### Description
Use SpecFlow instead of Reqnroll for Gherkin scenarios, keeping the rest of the toolchain the same.

### Benefits
- Longer track record and more existing tutorials/StackOverflow answers.

### Costs
- SpecFlow is in maintenance mode; its own team recommends migrating to Reqnroll, so choosing it now creates a migration debt on day one.

## NUnit instead of xUnit as the Reqnroll runner

### Description
Use Reqnroll.NUnit instead of Reqnroll.xUnit.

### Benefits
- Familiar to teams already standardized on NUnit.

### Costs
- No functional advantage for this solution's purpose; introduces a second test-runner convention into a repository that otherwise has no NUnit usage.
