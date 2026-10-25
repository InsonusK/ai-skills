# todo

The HTTP example of the base plateau in Python (`vp-http-api-in-python`) with a sample
**access** variation point attached through the extension points of
`base-plateau-extension-in-python`:

| Middleware | Kind | Where |
|---|---|---|
| `AuthenticationMiddleware` | API middleware (HTTP): the bearer token → the caller | `src/todo/api/http/middleware/authentication` |
| `AccessMiddleware` | command middleware, by the command's mark `Restricted` | `src/todo/command/middleware/access` |
| `BatchLimitMiddleware` | command middleware, by a value of the command (`Batch`) | `src/todo/command/middleware/batch_limit` |

`PlanItemsCommand` carries both marks (`src/todo/command/plan_items.py`); `src/todo/main.py`
lists the middleware. Nothing else of the HTTP example changed (`attach.allow`).

```bash
make init    # once: a virtual environment with the package and its dev tools
make run     # serve on :8080, callers t-alice (planner) and t-bob (no role), batch limit 3
make test    # every scenario
make lint    # the type check (mypy --strict)
```

```bash
curl -X POST localhost:8080/v1/items/plan -H 'Content-Type: application/json' -H 'Authorization: Bearer t-alice' -d '{"titles":["buy milk"]}'
# 200 {"items":[{"title":"buy milk","done":false}]}
curl -X POST localhost:8080/v1/items/plan -H 'Content-Type: application/json' -H 'Authorization: Bearer t-bob' -d '{"titles":["buy milk"]}'
# 403 {"code":7,"message":"PlanItems needs the role planner","details":[]}
curl -X POST localhost:8080/v1/items/plan -H 'Content-Type: application/json' -d '{"titles":["buy milk"]}'
# 401 {"code":16,"message":"PlanItems needs a caller","details":[]}
curl -X POST localhost:8080/v1/items/plan -H 'Content-Type: application/json' -H 'Authorization: Bearer t-alice' -d '{"titles":["a","b","c","d"]}'
# 429 {"code":8,"message":"PlanItems takes at most 3 items at once, got 4","details":[]}
```

| Setting | Default | Meaning |
|---|---|---|
| `ACCESS_TOKENS` | (none) | `token=user[:role+role]`, comma-separated: the known callers |
| `ACCESS_MAX_BATCH` | `100` | the most titles one `PlanItems` call may hold |
| `HTTP_LISTEN_PORT`, `HTTP_BASE_PATH`, `HTTP_API_DOCS_ENABLED`, `LOG_LEVEL` | | as in the HTTP example |
