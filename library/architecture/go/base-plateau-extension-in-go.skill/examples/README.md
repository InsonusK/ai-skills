# todo

The HTTP example of the base plateau in Go (`vp-http-api-in-go`) with a sample **access**
variation point attached through the extension points of `base-plateau-extension-in-go`:

| Step | Kind | Where |
|---|---|---|
| `access.Authenticate` | entry decorator (HTTP middleware): the bearer token → the caller | `internal/api/http/access` |
| `access.WithAccessCheck` | command decorator, by the command's mark `access.Restricted` | `internal/command/access` |
| `access.WithBatchLimit` | command decorator, by a value of the command (`access.Batch`) | `internal/command/access` |

`PlanItemsCommand` carries both marks (`internal/command/plan_items.go`); `cmd/todo/main.go`
lists the decorators. Nothing else of the HTTP example changed (`attach.allow`).

```bash
make init    # once: module dependencies
make run     # serve on :8080, callers t-alice (planner) and t-bob (no role), batch limit 3
make test    # every scenario
```

```bash
curl -X POST localhost:8080/v1/items/plan -H 'Authorization: Bearer t-alice' -d '{"titles":["buy milk"]}'
# 200 {"items":[{"title":"buy milk","done":false}]}
curl -X POST localhost:8080/v1/items/plan -H 'Authorization: Bearer t-bob' -d '{"titles":["buy milk"]}'
# 403 {"code":7,"message":"permission denied: PlanItems needs the role planner","details":[]}
curl -X POST localhost:8080/v1/items/plan -d '{"titles":["buy milk"]}'
# 401 {"code":16,"message":"unauthenticated: PlanItems needs a caller","details":[]}
curl -X POST localhost:8080/v1/items/plan -H 'Authorization: Bearer t-alice' -d '{"titles":["a","b","c","d"]}'
# 429 {"code":8,"message":"resource exhausted: PlanItems takes at most 3 items at once, got 4","details":[]}
```
