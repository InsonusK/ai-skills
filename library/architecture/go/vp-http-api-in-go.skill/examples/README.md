# todo

The base plateau in Go with the HTTP API variation point attached: the PlanItems command is
served as `POST /v1/items/plan`, with Swagger UI. Health endpoints come from the health
variation point (`vp-health-in-go`) on a port of their own.

```bash
curl -X POST localhost:8080/v1/items/plan -d '{"titles":["buy milk","walk the dog"]}'
```

```bash
make init    # once: module dependencies
make run     # build and serve on :8080 (Swagger UI at /swagger/)
make test    # every scenario
```
