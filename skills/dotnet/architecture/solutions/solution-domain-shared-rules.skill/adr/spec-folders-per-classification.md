---
name: spec-folders-per-classification
description: How each test project receives only the Domain.Rules.Spec scenarios it proves (the rule itself all, Domain.Tests @format, Application.Tests @semantic/@domain)
problem: Every consuming test project linked the shared .feature files as `<None Include>`, described as "filtered to the tags that project proves". Reqnroll generates no test for a `None` item, so no shared scenario ever ran — the offline-sync-service example built green while its three ItemTitle scenarios were never executed (the scenario report listed them as `missing`). Switching to `<ReqnrollFeatureFiles>` makes them run, but Reqnroll generates a test for every scenario of every file it is given — there is no per-tag filter at generation time — so a project would receive scenarios it has no step definitions for, and they would fail (and abort Stryker.NET's initial test run).
decision: Split Domain.Rules.Spec into format/, semantic/, and domain/ folders, one file per rule per classification. Each test project links whole folders as `<ReqnrollFeatureFiles>`: Domain.Rules.Tests all of them, Domain.Tests format/, Application.Tests semantic/ and domain/.
tags:
  - solution/domain-shared-rules
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The spec promises one Gherkin source re-proven at every layer that redirects to it. The linking mechanism never delivered that promise: `<None Include>` compiles cleanly and shows the file in the project, but Reqnroll only turns `ReqnrollFeatureFiles` items into tests. Fixing the item type exposes the second gap — the per-layer filtering by tag was only ever described, never implemented, and Reqnroll offers no build-time tag filter for which scenarios of a file become tests.

# Selected variant

**Selected variant:** [[#Folders per classification, linked by folder (selected)]]

# Searched variants

## Folders per classification, linked by folder (selected)

### Description

`{Module}.Domain.Rules.Spec/format/`, `semantic/`, `domain/`, one `{Rule}.feature` per rule per classification; every scenario still carries the matching `@format`/`@semantic`/`@domain` tag. `{Module}.Domain.Rules.Tests` links `**\*.feature`, `{Module}.Domain.Tests` links `format\**\*.feature`, `{Module}.Application.Tests` links `semantic\**\*.feature` and `domain\**\*.feature` — all as `<ReqnrollFeatureFiles Include=... Link=...>`.

### Benefits

- The filter is the build: a project never contains a scenario it has no step definitions for, so no skipped tests, no runtime hook, and Stryker.NET's initial run stays green.
- Verified on the offline-sync-service example: the three `format/ItemTitle.feature` scenarios now run in both `Domain.Rules.Tests` and `Domain.Tests` (8 → 14 tests), and the mutation score rose from 26.7% to 29.3% because they now kill mutants.
- The classification is visible twice — folder and tag — so an external consumer copying the spec sees which layer each file targets.

### Costs

- The exported spec's layout changes (`{Rule}.feature` → `{classification}/{Rule}.feature`); a consumer that copied the old flat layout must move its files once.
- A rule reused at two layers is described in two files instead of one.

## Whole files everywhere, skip other tags in a [BeforeScenario] hook

### Description

Every project links every file; a hook in `Domain.Tests`/`Application.Tests` calls `IUnitTestRuntimeProvider.TestIgnore` for scenarios whose tag belongs to another layer.

### Benefits

- The spec layout stays flat.

### Costs

- Every run reports skipped tests, so the TRX-based test count and badge need changing to stay green.
- One more hook per consuming project; forgetting it fails the build's tests with undefined steps.

## Keep `<None Include>`

### Description

Leave the linking as it was.

### Benefits

- No change.

### Costs

- No shared scenario is ever executed; the "re-proven at every layer" guarantee is false while every report looks green.
