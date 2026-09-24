---
name: xunit-v2-until-stryker-supports-xunit-v3
description: Why .NET conformance testing stays on xUnit v2 + Reqnroll.xUnit on the VSTest runner instead of xunit.v3 on Microsoft.Testing.Platform, and how to re-check that when Stryker.NET is updated
problem: xunit.v3 on Microsoft.Testing.Platform (MTP) is xUnit's current line and the direction of `dotnet test` on the .NET 10 SDK, and the dotnet plateau examples had moved to it. Stryker.NET 4.16.0 runs such a solution without an error but reports every mutant as survived — a false 0% mutation score — while the same code and tests on xUnit v2 / VSTest score 55%.
decision: Stay on xUnit v2 (xunit 2.9.x, xunit.runner.visualstudio 3.x, Reqnroll.xUnit) on the VSTest runner, with coverlet.collector, until recheck/stryker-xunit-v3.sh reports SUPPORTED for a released Stryker.NET version.
tags:
  - solution/conformance-testing-in-dotnet
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

> **Note for agents:** do not move this solution, or any .NET example built on it, to xunit.v3 / Microsoft.Testing.Platform before running [[skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/recheck/stryker-xunit-v3.sh|recheck/stryker-xunit-v3.sh]] and getting `SUPPORTED`. The failure is silent: `make mutation-test` exits 0 and publishes a 0% mutation score, which reads as "the tests assert nothing" rather than as a tooling fault.

Measured on 2026-09-24 (the 5.0.0 rows by `recheck/stryker-xunit-v3.sh` itself) against the plateau-core example (same production code, same tests, `test-case-filter: Category!=todo`), .NET 10 SDK:

| Stack | Stryker.NET | Killed | Survived | No coverage | Score |
| --- | --- | --- | --- | --- | --- |
| xUnit 2.9.3 + Reqnroll.xUnit 3.3.4, VSTest | 4.16.0 | 11 | 4 | 5 | 55% |
| xunit.v3 4.0.0 + Reqnroll.xunit.v3 3.3.4, MTP | 4.16.0 | 0 | 20 | 0 | 0% |
| xUnit 2.9.3 + Reqnroll.xUnit 3.3.4, VSTest | 5.0.0 | 11 | 4 | 5 | 55% |
| xunit.v3 4.0.1 + Reqnroll.xunit.v3 3.3.4, MTP | 5.0.0 | 0 | 20 | 0 | 0% |

On MTP, Stryker logs `It looks like the test coverage capture failed` and then counts every mutant as survived. The untouched MTP example (with `coverlet.collector`) and a variant using `Microsoft.Testing.Extensions.CodeCoverage` both gave 0%, so the coverage package is not the cause.

On MTP, `WITH_CODE_COVERAGE=true` also needs `Microsoft.Testing.Extensions.CodeCoverage` instead of `coverlet.collector` (a VSTest data collector) — take that into account when the switch becomes possible.

# Selected variant

**Selected variant:** [[#xUnit v2 on VSTest until Stryker.NET supports xunit.v3 (selected)]]

# Searched variants

## xUnit v2 on VSTest until Stryker.NET supports xunit.v3 (selected)

### Description

Test projects reference `xunit`, `xunit.runner.visualstudio` 3.x, `Reqnroll.xUnit`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`; no `global.json` test-runner opt-in and no `UseMicrosoftTestingPlatformRunner`. Re-check on every Stryker.NET release:

```bash
# from the repository root; needs the .NET 10 SDK, jq, curl, network access to NuGet
skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/recheck/stryker-xunit-v3.sh
# pin versions explicitly if needed:
STRYKER_VERSION=4.17.0 XUNIT_V3_VERSION=4.0.0 \
  skills/dotnet/test/solution-conformance-testing-in-dotnet.skill/recheck/stryker-xunit-v3.sh
```

The script copies the fixture twice, converts one copy to xunit.v3/MTP, runs the chosen Stryker.NET on both, and compares killed/survived counts. Exit `0` `SUPPORTED`, `1` `NOT SUPPORTED`, `2` `INCONCLUSIVE` (a build failed — read the log it names — or v2 killed nothing).

When it reports `SUPPORTED`: record a new ADR superseding this one, move the testing tool list and `templates/unit-test.sh.md` to xunit.v3/MTP (`--filter-not-trait "Category=todo"`, counts from the MTP summary), switch coverage to `Microsoft.Testing.Extensions.CodeCoverage`, and propagate through `solution-dotnet-conformance-testing`, `solution-central-package-management`, and the dotnet plateau examples.

### Benefits

- Mutation testing — a MUST of [[skills/common-workflow/test/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] — gives a real score.
- The stack is verified end to end: build, `unit-test` with coverage, `mutation-test`, scenario report.

### Costs

- xUnit v2 is in maintenance mode; new xUnit features land in v3 only.
- VSTest is no longer the default direction of `dotnet test`; a later migration is certain.

## xunit.v3 on Microsoft.Testing.Platform now

### Description

Keep the plateau examples on xunit.v3/MTP and accept Stryker.NET's current behavior.

### Benefits

- Current xUnit line and `dotnet test` direction.

### Costs

- Every mutation report is a false 0%; the mutation gate is effectively switched off while looking active.

## xunit.v3 on MTP, mutation testing disabled

### Description

Move to xunit.v3 and drop `make mutation-test` for .NET until Stryker.NET catches up.

### Benefits

- Current xUnit line; no false report.

### Costs

- Violates the parent solution's rule that mutation testing is part of every stack's gate; .NET would be the only stack without it.
