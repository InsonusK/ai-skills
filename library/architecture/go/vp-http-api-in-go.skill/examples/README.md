# todo

[![tests](https://img.shields.io/endpoint?url=https://example.github.io/todo/testing/badges/tests.json)](https://example.github.io/todo/testing/reports/tests/)
[![coverage](https://img.shields.io/endpoint?url=https://example.github.io/todo/testing/badges/coverage.json)](https://example.github.io/todo/testing/reports/coverage/)
[![mutation](https://img.shields.io/endpoint?url=https://example.github.io/todo/testing/badges/mutation.json)](https://example.github.io/todo/testing/reports/mutation/)

The base plateau in Go with the HTTP API variation point attached: the PlanItems command is
served as `POST /v1/items/plan`, with health endpoints and Swagger UI.

```bash
curl -X POST localhost:8080/v1/items/plan -d '{"titles":["buy milk","walk the dog"]}'
```

```bash
make init              # once: module dependencies and the mutation tool
make run               # build and serve on :8080 (Swagger UI at /swagger/)
make test-kind-unit    # every scenario
make test-and-report   # every kind, then the report: tmp/testing/report/index.html
```
