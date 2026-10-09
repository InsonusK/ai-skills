# Angular Link checker

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/tests.json)](https://example.github.io/angular-linkcheck/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/coverage.json)](https://example.github.io/angular-linkcheck/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/mutation.json)](https://example.github.io/angular-linkcheck/testing/reports/mutation/)
[![components](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/components.json)](https://example.github.io/angular-linkcheck/testing/reports/components/)
[![ui](https://img.shields.io/endpoint?url=https://example.github.io/angular-linkcheck/testing/badges/ui.json)](https://example.github.io/angular-linkcheck/testing/reports/ui/)

This Angular application validates and normalizes a URL through the same real domain function exercised by the inherited TypeScript Cucumber suite. The form supports button/keyboard submission, validation errors and resubmission.

Use the supplied [.devcontainer](.devcontainer/devcontainer.json), which provisions Node 24, Make, jq, Chromium and its system libraries, or a host with the same prerequisites. `make init` installs the committed lockfile, Chromium and its system libraries. On Linux, system-package installation requires root or sudo; initialization fails if it cannot prepare the runtime. In a prepared environment, run `make init && make test-and-report`; open `tmp/testing/report/index.html`.

| Make command | Result |
| --- | --- |
| `make test-kinds` | Discover the four test kinds and their five badges |
| `make test-kind-unit` | Cucumber scenarios, domain coverage and living documentation |
| `make test-kind-mutation` | Domain-only Stryker mutation report |
| `make test-kind-components` | Angular DOM behavior and native component coverage |
| `make test-kind-ui` | Browser interactions and visual comparison; starts/stops the app server |
| `make test-report` | Assemble the results already produced |
| `make test-and-report` | Run every kind and assemble the report, even if a kind fails |

CI calls these same commands. Publish `tmp/testing/report/` as a static artifact; it contains the index, badges, coverage, mutation, living documentation and native browser evidence. The badge URLs above illustrate a publication destination; local results appear when the Makefile runs.

For a check run with caller-selected output paths, use `make test-and-report TEST_RUN_PURPOSE=check TEST_WORK_DIR=out/work TEST_REPORT_DIR=out/report`. Cucumber/components/UI still run; mutation skips without `DELTA_BASE`, and domain coverage is collected without being published. For changed domain mutation, provide `DELTA_BASE=<commit>` to `make test-kind-mutation TEST_RUN_PURPOSE=check`.

The reviewed Linux/Chromium visual baseline is committed beside the UI spec. Normal tests never update it. Follow the owning skill's baseline-review procedure when intentionally changing the UI or execution environment.

To explore the app manually, run `npm run start`. Its production build is `npm run build`; testing remains behind Make.

The example source-contract check is `bash ../scripts/check-example.sh`; recorded executions and environment limitations are in the owning skill’s verification document.
