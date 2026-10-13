# linkcheck

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/tests.json)](https://example.github.io/linkcheck/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/coverage.json)](https://example.github.io/linkcheck/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/linkcheck/testing/badges/mutation.json)](https://example.github.io/linkcheck/testing/reports/mutation/)

A runnable linkcheck module with eight co-located features: domain checks, extraction, batch summary, CLI, file store, mapping, result contract and version metadata. The tests badge opens living documentation with every scenario, its tags and excluded reason.

```bash
make init              # once: module dependencies and the mutation tool
make test-kinds        # the kinds and the badges each declares
make test-and-report   # every kind, then the report: tmp/testing/report/index.html
```
