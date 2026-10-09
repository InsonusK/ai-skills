# Angular Nx Link checker

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/tests.json)](https://example.github.io/angular-linkcheck/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/coverage.json)](https://example.github.io/angular-linkcheck/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/mutation.json)](https://example.github.io/angular-linkcheck/testing/reports/mutation/)
[![components](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/components.json)](https://example.github.io/angular-linkcheck/testing/reports/components/)
[![ui](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/ui.json)](https://example.github.io/angular-linkcheck/testing/reports/ui/)

This Nx workspace validates and normalizes a URL through the same real domain function exercised by the inherited TypeScript Cucumber suite. The form supports button/keyboard submission, validation errors and resubmission.

Use the supplied [.devcontainer](.devcontainer/devcontainer.json), which provisions Node 24, Make, jq, Chromium and its system libraries, or a host with the same prerequisites. `make init` installs the committed lockfile, Chromium and its system libraries. On Linux, system-package installation requires root or sudo; initialization fails if it cannot prepare the runtime. In a prepared environment, run `make init && make test-and-report`; open `tmp/testing/report/index.html`.

| Make command | Result |
| --- | --- |
| `make test-kinds` | Discover the four test kinds and their five badges |
| `make test-kind-unit` | Cucumber scenarios, domain coverage and living documentation |
| `make test-kind-mutation` | Domain-only Stryker mutation report |
| `make test-kind-components` | Angular DOM behavior and native component coverage |
| `make test-kind-ui` | Browser interactions and visual comparison; starts the app server on a free port and stops it |
| `make test-report` | Assemble the results already produced |
| `make test-and-report` | Run every kind and assemble the report, even if a kind fails |

CI calls these same commands. Publish `tmp/testing/report/` as a static artifact; it contains the index, badges, coverage, mutation, living documentation and native browser evidence. The badge URLs above illustrate a publication destination; local results appear when the Makefile runs.

For a check run with caller-selected output paths, use `make test-and-report TEST_RUN_PURPOSE=check TEST_WORK_DIR=out/work TEST_REPORT_DIR=out/report`. Without DELTA_BASE all applicable Cucumber/components/UI projects still run; mutation skips without `DELTA_BASE`, and domain coverage is collected without being published. For changed domain mutation, provide `DELTA_BASE=<commit>` to `make test-kind-mutation TEST_RUN_PURPOSE=check`.

The reviewed Linux/Chromium visual baseline is committed beside the UI spec, in `apps/portal-e2e/src/__screenshots__/`. Normal tests never update it. Follow the owning skill's baseline-review procedure when intentionally changing the UI or execution environment.

The workspace was created with the official tooling — `create-nx-workspace --preset=angular-monorepo`, then `nx g @nx/angular:application`, `nx g @nx/angular:library` and `nx g @nx/js:library` — and holds four projects: `apps/portal`, its browser-test project `apps/portal-e2e`, `libs/linkcheck` (a component and the domain showcase) and `libs/formatter` (logic only). Nothing about testing is declared in a `project.json`: a kind reads the workspace as Nx sees it.

| Kind | Runs for a project that has | Through |
| --- | --- | --- |
| `unit` | a `.feature` file | `cucumber-js`, the project's features only |
| `components` | the standard `test` target | `nx run {project}:test` |
| `ui` | the standard `e2e` target | `nx run {project}:e2e` |
| `mutation` | — | Stryker over the `mutate` patterns of `stryker.conf.json` |

A developer's own commands keep working beside `make`: `nx test linkcheck`, `nx e2e portal-e2e`, `npx cucumber-js`.

A `report` run takes every project a kind applies to. A `check` run with `DELTA_BASE=<commit>` takes the projects `nx show projects --affected` names — dependencies come from the imports, so a change in `libs/formatter` selects `apps/portal` and `apps/portal-e2e` too; without a base it takes all. Scenarios of an unaffected project stay in the living documentation as `not-run`. Every Nx call skips the task cache. The component and UI reports link each project's log, coverage and Playwright report; `reports/tests/projects.html` shows the unit selection.

`python3 ../scripts/check-nx.py` proves the selection and the failure cases on a two-commit copy. The whole contract: `bash skills/testing/agent/run-example.sh <example-dir>` from the repository root.
