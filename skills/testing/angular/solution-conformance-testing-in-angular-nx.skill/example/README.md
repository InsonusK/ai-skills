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

The reviewed Linux/Chromium visual baseline is committed beside the UI spec. Normal tests never update it. Follow the owning skill's baseline-review procedure when intentionally changing the UI or execution environment.

The workspace has `apps/portal`, `libs/linkcheck` (the component and inherited domain showcase) and `libs/formatter` (domain logic). The application has its own scenarios beside `src/logic`. Every project declares test applicability in `project.json`; the formatter explicitly has no component/browser boundary.

A report runs every applicable project. A check with `DELTA_BASE=<commit>` runs affected unit/component/UI targets; without a base it runs all applicable projects. Unaffected scenarios remain visible as `not-run` in the full living documentation. Mutation retains the parent's changed-file scope. Every Nx invocation skips local and remote task caches. Component/UI reports link per-project logs, coverage and Playwright evidence; `reports/tests/projects.json` describes unit selection.

Run `python3 ../scripts/check-nx.py` for a two-commit affected fixture, cache-bypass failure, missing output and empty-project checks. Targets use Nx core and native Angular builders directly, without a second `angular.json` source of project configuration. Full contract verification: run `bash skills/testing/agent/run-example.sh <example-dir>` from the repository root.
