# Todo

The HTTP example of the base plateau in dotnet (`vp-http-api-in-dotnet`) with a sample
**access** variation point attached through the extension points of
`base-plateau-extension-in-dotnet`:

| Step | Kind | Where |
|---|---|---|
| `Authentication` | entry decorator (ASP.NET `IMiddleware`): the bearer token → the caller | `src/App/App.Host/Api/Http/Access` |
| `AccessCheckBehavior<,>` | command decorator, by the command's mark `IRestricted` | `src/Access` |
| `BatchLimitBehavior<,>` | command decorator, by a value of the command (`IBatch`) | `src/Access` |

The marks live in `src/Shared/Access`; `PlanItemsCommand` carries both
(`src/Modules/Todo/Todo.Commands/PlanItemsCommand.cs`); `Program.cs` lists the decorators and
the middleware. Nothing else of the HTTP example changed but the project files (`attach.allow`).

```bash
make init    # once: the packages of every project
make run     # serve on :8080, callers t-alice (planner) and t-bob (no role), batch limit 3
make test    # every scenario
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
