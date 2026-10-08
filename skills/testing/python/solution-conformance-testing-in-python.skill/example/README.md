# linkcheck

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/tests.json)](https://example.github.io/linkcheck/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/coverage.json)](https://example.github.io/linkcheck/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/mutation.json)](https://example.github.io/linkcheck/testing/reports/mutation/)

The minimal package `solution-conformance-testing-in-python` was proved on: one feature, one plain test, both test kinds.

```bash
make init              # once: .venv with the dev dependencies
make test-kinds        # the kinds and the badges each declares
make test-and-report   # every kind, then the report: tmp/testing/report/index.html
```
