# linkcheck

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/tests.json)](https://example.github.io/linkcheck/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/coverage.json)](https://example.github.io/linkcheck/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/mutation.json)](https://example.github.io/linkcheck/testing/reports/mutation/)

Run `make init && make test-and-report` and open `tmp/testing/report/index.html`. The tests link opens the living doc with every scenario, tag and exclusion reason. Features and steps sit beside their production modules in `src/linkcheck/`; `npm run build` excludes steps from the distribution.
