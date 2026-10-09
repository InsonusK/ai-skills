# .NET test layout migration contract

Task: [TASK-dotnet-layout.md](TASK-dotnet-layout.md). Base: `skills-testing`; branch: `dotnet-test-layout`.

## Invariants for review

1. `cucumber-testing-in-dotnet` owns the test-project folder rule: `features/` and `Steps/`. Convert it to a folder skill to hold the layout ADR; update every incoming link. Other skills link the rule instead of restating the directory tree.
2. Testing skills own packages, runner configuration, mixed unit/scenario execution, production-code calls, and concrete-error assertions. Catalog solutions and derived plateau skills link the owning rules; remove duplicated generic templates. Preserve layer-specific validator, command, contract, value, and pipeline step shapes.
3. The catalog keeps project selection and architectural reference boundaries: Domain tests only with VP1, Domain.Rules tests only with VP4, no Api tests, and production Allowed Dependencies mirrored by each test project. Preserve unrelated solutions' contributions and existing `created_by` provenance.
4. Update the solution first. Propagate its changed Implementation contributions through plateau-core, plateau-domain-service, and plateau-offline-sync-service, including their root skills and descendants. Raise every changed skill version; record decisions as ADRs in the owning skills.
5. Rename example folders and align project items, formatter paths, namespaces, Stryker configuration, and message-file discovery. Production behavior, scenario inventories, coverage, and mutation scores must remain unchanged. Keep stack kind-script copies identical to their assets if an asset changes.
6. Edit only this worktree's `skills/` sources. Angular and DevOps are outside scope. Keep separate test projects and the existing shared testing contract.

## Waves and artifact inventory

| Wave | Artifacts | Operation | Gate |
| --- | --- | --- | --- |
| 1: solution | .NET Cucumber skill and incoming links; testing Repository.extend; catalog root and five project/five step Implementation files; owner ADRs | Move Cucumber skill; modify solution; create ADRs | testing check.sh; affected-link and duplicate-rule audit |
| 2: plateaus | Three root skills and every structure skill derived from the changed solution, including VP-specific test contributions | Propagate solution diff; preserve other contributors | testing check.sh; provenance, version, boundary, link and duplicate-rule audit |
| 3: examples | Three plateau examples and the .NET showcase; any required stack assets and their copies | Rename and align configuration | testing check.sh; run-example.sh for each of four examples; compare metrics |

Each wave receives a separate conformance audit and commit. A task-specific mechanical checker will enforce affected-link resolution, folder vocabulary, version/provenance preservation, and wave coverage alongside the existing testing check.

## Ground truth

| Example | Tests | Coverage | Mutation |
| --- | --- | --- | --- |
| plateau-core | 7/7 | 72.4% | 55.0% |
| plateau-domain-service | 10/10 | 41.7% | 31.1% |
| plateau-offline-sync-service | 14/14 | 40.1% | 29.3% |
| showcase | 25/25 | 100% | 94.4% |

Known Stryker coverage-capture warnings in plateau-core are documented in the task; test discovery and scores remain the regression gates.

## Decisions log

- 2026-10-09: Folder vocabulary and catalog/testing ownership follow the owner's task decisions; no new architecture fork is proposed.
- 2026-10-09: Use the testing harness plus a task-specific checker. The catalog checker still points to the retired `architecture/v3.1` tree and cannot validate this migration; repairing that unrelated harness is outside scope.
- 2026-10-09: Propagate changes as merges of the existing solution contributions; preserve other solution contributions.

## Status

- Separate branch/worktree created from `skills-testing` at `b3570c54`; base pushed (already up to date).
- Baseline `bash skills/testing/agent/check.sh`: passed.
- .NET, jq, and Node are available.
- Contract reviewed by the owner on 2026-10-09; bulk work authorized.
- Wave 1: solution and stack owners updated; testing check and task-specific wave gate passed; separate conformance audit completed.
- Wave 2: three root skills and 33 structural skills refreshed through the parent chain; generic class templates and package tables removed; other solution provenance and VP4/Cecil contributions preserved. Both gates passed.
- Example wave and final pull request into `skills-testing`: pending.

- Wave 1 decision: VP4 solution also produced the obsolete layout; updated its rule-test contribution before propagating to plateaus. Recorded its own ADR.
- Wave 1 audit: corrected an unintended production-path substitution before commit; production paths are preserved. Moved the generic production-binding rule into the .NET Cucumber owner.

- Wave 2 audit: found and removed a composite Application bullet still repeating unit/scenario and assertion rules. Confirmed VP1/VP4 selection, all contributor lists, shared-spec XML links, and the separate rule-only/Domain Cecil scopes. No conflict resolution, solution removal or structural-skill deletion required a plateau ADR.

- Boundary audit correction: old catalog lists did not actually mirror production Allowed Dependencies. Replaced them with production-skill links, preserving test layer entry points and the existing Cecil scan extension. Source ADR updated; each plateau records this conflict resolution and the full propagation set.
- Completed example runs so far: core 7/7, 72.4%, 55.0%; domain-service 10/10, 41.7%, 31.1%; both harnesses passed A/B/C and report-link checks.
