# Angular repository shapes — review contract

Status: preparation complete; owner review pending before bulk authoring.
Task: [TASK-angular-shapes.md](TASK-angular-shapes.md).
Base: `skills-testing` at `b3570c54`; task branch: `angular-testing-shapes`.
Work stays local: no push or PR, per [TASK.md](TASK.md).

## Invariants

1. Exactly one entry skill applies by repository shape: the existing Angular skill for one application (external npm dependencies do not change its shape), the library refinement for one published library, and the Nx refinement for a workspace containing its applications and libraries. Both refinements depend on the existing Angular skill.
2. Preserve the [caller contract](INVARIANTS.md): four kinds (`unit`, `mutation`, `components`, `ui`), one repository report, existing targets and caller variables, living documentation containing the complete scenario inventory and tags. No project-specific badges; project identity stays visible in kind evidence.
3. Reuse the Angular result adapter, UI config template, component/UI spec rules and glossary. Shared core tools and TypeScript `mutation.sh` remain byte-identical to their original assets. Put replacement kind scripts/configuration in refinement assets and copy them verbatim into examples.
4. Native specs stay in adjacent `spec/`; Cucumber `features/` and `test/` stay beside the logic in any project, including an Nx application. Production compilation and published library contents exclude every test artifact.
5. The library example has a publishable component and domain logic, with a `projects/demo` browser host following the existing design-system convention. `npm pack --dry-run` must prove the built package contains no specs, steps or features.
6. The Nx example has one application and at least two libraries. App and library domain logic both have real scenarios. Every project declares its applicable kinds explicitly: a missing suite for a declared kind is a failure; an explicitly inapplicable kind is visible in project inventory. An empty selected suite cannot produce a green kind.
7. Nx runners bypass cache. A changed, deliberately failing spec between two runs must fail the second run. Per-project evidence retains project identity through aggregation, including failure and missing-output cases.
8. UI servers pick a free `UI_TEST_PORT` per run; no configured fixed port. Normal runs cannot update reviewed, committed screenshot baselines. Missing browsers/results and empty suites fail.
9. Report mutation scope includes framework-independent logic in applications and libraries. Check mutation retains the parent runner's changed-file scope within configured `mutate` patterns, rather than expanding to every file of an affected project.
10. Proposed native-kind delta policy: `components` and `ui` select `nx affected` only for `check` with `DELTA_BASE`; otherwise run all applicable projects. Unit selection must retain a complete, truthfully marked scenario inventory. No affected projects means an explicit skip with reason and mode, never a green empty suite.
11. Catalog edits only replace the two missing-Nx-adapter sentences with a link to the Nx refinement. DevOps receives known gaps only (cache and browser image). No workflow or devcontainer changes.
12. Ground truth contains measured counts, coverage/mutation, cold initialization plus report runtime, and unverified items. Docker image builds cannot be verified here. A cold run over ten minutes requires a concrete proposal to reduce its cost.

## Waves and artifact ownership

| Wave | Artifacts | Treatment | Required evidence |
| --- | --- | --- | --- |
| 0 | This contract and existing harness baseline | New contract; reuse harness | Existing `check.sh` passes; owner reviews contract |
| 1 | Angular library skill, Implementation, ADRs, assets and example | New refinement; reuse base assets/spec rules | `run-example.sh`, package dry run, native failure cases, measured cold run |
| 2 | Angular Nx skill, Implementation, ADRs, assets and example | New refinement; reuse base adapter; aggregation as needed | `run-example.sh`, two-commit affected fixture, cache-bypass failure, empty/missing project evidence, measured cold run |
| 3 | Base selection/boundaries, `check.sh` §4/§10, two catalog solutions, DevOps gaps | Targeted changes | Full mechanical check, both new example checks, base example rerun if its delivery changed |

Each wave receives a separate conformance review and commit after its checks pass. Tracking stays in this agent document; skills carry rules and measured ground truth, not work logs. Architecture choices are recorded in the owning skill's ADRs in the established format.

## Mechanical and execution gates

- `bash skills/testing/agent/check.sh` covers links, dependencies, layout, shared copies and the make/report contract. Extend §4 for the two owner-decided shape suffixes (`-in-angular-library`, `-in-angular-nx`), keeping strict naming for all other skills; extend §10 to compare refinement scripts with their owning assets and inherited originals.
- `bash skills/testing/agent/run-example.sh <example-dir>` exercises report, caller-chosen check directories, delta mutation, living documentation and report links for each refinement.
- Add focused executable assertions for packaging, Nx aggregation and affected selection, missing/empty evidence, cache bypass and the scenario inventory. Assertions must inspect actual runner evidence, not only configuration text.
- Deliberate test breakage is restored from a saved copy. No changes to other worktrees or generated skill copies.

## Decisions and owner review

- Decided by owner: three skills by repository shape; scenarios beside logic in both applications and libraries.
- Recommendation: one badge per kind; project breakdown inside the report.
- ⚠️ Confirm invariant 9: full domain mutation across app/library projects in report runs; changed source files only in check runs. The unchanged TypeScript runner uses `git diff` filtered by Stryker patterns, so mutating whole affected projects would contradict the task's requirement to retain that runner.
- ⚠️ Confirm invariant 10: affected native kinds in delta check runs change the base's always-full behavior and require the owner's decision.
- Recommendation: a project explicitly declares applicability; absent tests for an applicable kind fail. Non-UI domain libraries do not need dummy browser tests.
- Implementation detail to verify: how Nx test targets expose native output for the pinned Angular/Nx versions. No measured compatibility claim before execution.

## Preparation evidence

- Created the dedicated worktree from the requested base, with a clean initial tree.
- `bash skills/testing/agent/check.sh`: exit 0, all checks passed.
- Node: `v24.21.0`; available disk exceeds 200 GB.
- No new skill or runnable example authored before owner review.
