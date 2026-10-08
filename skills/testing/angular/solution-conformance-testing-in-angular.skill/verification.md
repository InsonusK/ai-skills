# Verification — 2026-10-08

# Contract and scope

The [runnable Angular example](example/README.md) is the source of truth: a URL form consumes the unchanged TypeScript domain package and its eight adjacent Gherkin features. All four kinds use one Makefile and the shared report assembler. Testing consumers invoke Make, without knowing runner commands or result formats. Common core assets are copied verbatim; the TypeScript mutation delta selector is narrowed to `src/linkcheck/` so changed component files cannot enter domain mutation.

Copied unchanged: TypeScript domain sources/features/steps, Cucumber config/unit adapter, shared core/livingdoc assets. Adapted: package/lockfile, Angular configs, domain mutation scope, README and initialization. New: Angular component/template/styles, component and browser specs, reviewed screenshot baseline and environment recipe. The loose counter illustrations have been replaced by this complete application; the decision is in [Complete Makefile example](adr/complete-makefile-example.md).

# Executed public commands

`make init` and `make test-and-report` exited 0 in `example/` and generated `tmp/testing/report/index.html`.

| Evidence | Actual result |
| --- | --- |
| Cucumber/domain | 25/25 scenarios, 76 executed Gherkin steps |
| Domain line coverage | 98.55% |
| Domain mutation | 90.2%; 101 killed, 11 survived |
| Angular component behavior | 4/4 passed |
| Native TestBed line coverage | 100%, covering the component, template and invoked domain code |
| Browser UI | 4/4 passed, including button/keyboard submission, error recovery and visual comparison |
| Report/living documentation | 12 linked pages, zero broken links; 25 scenario inventory entries with tags/reasons |

`make test-and-report TEST_RUN_PURPOSE=check TEST_WORK_DIR=out/work TEST_REPORT_DIR=out/report` exited 0. Component/UI/Cucumber kinds ran, domain mutation skipped without a delta base, and the domain coverage badge/report were omitted. Native component coverage stayed under its own report. Both report trees passed the shared report-link and living-documentation checks.

A deliberately incorrect component expectation was then tested through the same `make test-and-report` command in check mode under `out/negative/`. It returned nonzero, published a red 3/4 component badge, still executed the UI and domain kinds and assembled `out/negative-report/index.html`. The original spec was restored. The committed visual baseline stayed byte-identical during normal executions.

`npm run build` produced the Angular application under `dist/linkcheck/` with only production browser code, CSS and licensing output. The [cheap source-contract check](scripts/check-example.sh) verifies core/Angular asset identity, unchanged inherited domain sources, filled config templates, domain delta adaptation, baseline presence, Make discovery, badges and repository links. An independent read-only audit found no blocking defect in the Makefile contract or source boundaries.

# Runtime boundary

Tested: Node 24.21, Angular core/compiler 22.2.1, Angular CLI/build 22.2.2, TypeScript 6.0.2, Vitest/coverage-v8 5.0.3, jsdom 30.1.2 and Playwright 1.64.0 with matching Chromium. This workspace lacks Chromium system libraries; the runs used runtime libraries previously extracted into `/tmp/angular-chromium-libs` via `LD_LIBRARY_PATH`. That is environment preparation, not an extra testing caller parameter. The supplied devcontainer provisions the browser/system prerequisites, but its Docker build was not executed because this workspace has no Docker binary.

Earlier adapter-level proof also verified red results for an empty component suite, missing native JSON, missing browser libraries and missing visual baseline; missing baselines were not silently created. Multi-application aggregates and older Angular executors remain outside the verified scope.
