# todo

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/todo/testing/badges/tests.json)](https://example.github.io/todo/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/todo/testing/badges/coverage.json)](https://example.github.io/todo/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/todo/testing/badges/mutation.json)](https://example.github.io/todo/testing/reports/mutation/)

The base plateau in Go: the process starts, runs its one startup command — plan a list of
to-do items from `STARTUP_TITLES` — prints the result as JSON and exits.

```bash
make init              # once: module dependencies and the mutation tool
make run               # build and run the startup command
make test-kind-unit    # every scenario
make test-and-report   # every kind, then the report: tmp/testing/report/index.html
```
