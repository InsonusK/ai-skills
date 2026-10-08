# Verification — 2026-10-08

The delivered configs/scripts were copied into a temporary real Angular CLI counter application; its sources are preserved in [examples/](examples/README.md). Tested versions: Node 24.21, Angular core/compiler 22.2.1, Angular CLI/build 22.2.2, TypeScript 6.0.2, Vitest/coverage-v8 5.0.3, jsdom 30.1.2, Playwright 1.64.0 and matching Chromium. Missing Linux runtime libraries were extracted into a temporary directory for this proof rather than installed into the system.

| Case | Actual result |
| --- | --- |
| Component report run | 2/2 passed; native component line coverage 100% |
| Component check run in caller-selected directories | 2/2 passed; JSON and coverage stayed under that kind |
| Browser report and check runs | 2/2 passed, including the visual comparison |
| Wrong component DOM expectation | 1/2 passed; nonzero kind, red badge and failure evidence |
| Empty component suite | Nonzero kind; red badge and readable report |
| Wrong browser interaction expectation | 1/2 passed; nonzero kind and red report with native trace/screenshot evidence |
| Chromium missing a system library | Nonzero kind and readable native runtime failure |
| Missing visual baseline | 1/2 passed; no automatic baseline creation |
| Adapter invoked without native JSON | Nonzero result; red badge, no green zero/zero |
| Normal browser runs after baseline review | Baseline SHA-256 unchanged |

`make test-kinds` discovers all four kinds after copying the inherited TypeScript adapters. `make test-readme-check` accepts their five badge declarations. The unchanged `make test-report` successfully assembles both successful and failed native runs. Component/UI summary links and their native coverage/report entry pages resolve in the assembled static report. The domain delta selector was checked in an isolated Git fixture: a UI-only change selects nothing, while a domain change selects its actual source file.

Shell syntax, adapter syntax and the repository's `skills/testing/agent/check.sh` pass.

The inherited domain Cucumber/mutation suites were not rerun in this temporary application; they belong to the existing TypeScript solution and were not changed. Multi-application aggregates and older Angular executors were not verified by this proof.
