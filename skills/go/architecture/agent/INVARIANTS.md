# skills/go/architecture catalog invariants

The anchor document for this catalog's build (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Every artifact in `solutions/` and `plateau/` must satisfy every invariant here. `check.sh` enforces the mechanical ones; the per-wave audit enforces the rest.

## 1. Baseline (what exists before any optional feature)

```
go.mod
Makefile                          ← solution-go-repository-structure (build/run/lint) +
                                     solution-conformance-testing-in-go (test-kind-unit/
                                     test-kind-mutation/test-report-build)
.gitignore
report-template/index.html        ← solution-conformance-testing-in-go
cmd/
  {service}/
    main.go                       ← solution-go-repository-structure (skeleton) → extended by
                                     every other solution applied
internal/
  config/
    config.go                     ← solution-go-repository-structure (loader, empty Config)
  version/
    version.go                    ← solution-go-repository-structure
  logging/
    logger.go                     ← solution-go-app-logging
  domain/
    services/
      {service}.go                ← solution-go-domain-logic (zero fields/deps at baseline)
      features/                   ← Cucumber .feature files, co-located per package
      test/                       ← godog step definitions + World
  api/
    http/
      server.go                   ← solution-go-http-api (Options, contract routes, handlers)
      docs.go                     ← solution-go-http-api (spec and Swagger UI routes)
      errors.go                   ← solution-go-http-api (the error body)
      features/                   ← the HTTP API feature, @type/api/http
      test/
docs/
  integration/
    api/
      {contract-name}/
        openapi.json              ← solution-go-http-api (hand-written contract)
        embed.go                  ← solution-go-http-api
tools/
  normalize_unittest/main.go      ← solution-conformance-testing-in-go
  normalize_scenarios/main.go     ← solution-conformance-testing-in-go
  normalize_mutation/main.go      ← solution-conformance-testing-in-go
  test_report/main.go             ← solution-conformance-testing-in-go
```

- **No `internal/domain/interfaces/`** at baseline — created by `solution-go-domain-ports`, a shared prerequisite the first-applied of `solution-external-integration`/`solution-cached-db`/`solution-persistent-db` depends on (VP-C005/VP-C002/VP-C001).
- **No `internal/infrastructure/`, `internal/api/grpc/`, `buf.yaml`, `buf.gen.yaml`, `gen/`, `docs/integration/client/`** at baseline — each created by its own VP-realizing solution. `proto/` and `buf/` exist in no plateau: see [§9](#9-api-contracts-issue-206).
- Every Go module dependency is pinned once in `go.mod` — no separate central-package-management solution exists for this stack (unlike the dotnet catalog); see `feature/feature-model.md`'s "Modeling choices" section.

## 2. Common features → realizing solution (must be 1:1 covered)

| Feature (feature-model.md) | Solution |
| --- | --- |
| (baseline repo/composition-root structure) | `solution-go-repository-structure` |
| (shared outbound-ports package prerequisite) | `solution-go-domain-ports` — not a Feature Model row itself, a shared prerequisite of VP-C005/VP-C002/VP-C001 |
| DomainLogic | `solution-go-domain-logic` |
| HttpApi | `solution-go-http-api` |
| AppLogging | `solution-go-app-logging` |
| TestConformance (+ Cucumber / Code Coverage / Mutation / Test Reports) | `solution-conformance-testing-in-go` |

## 3. Variation Points → realizing solution(s) (must be 1:1 covered)

| VP | Solution(s) | Constraint (from variability-map.md) |
| --- | --- | --- |
| VP1 GrpcApi | `solution-grpc-api` | — |
| VP-C005 GrpcOutbound (ExternalIntegration) | `solution-external-integration` (`depends_on` `solution-go-domain-ports`) | — |
| VP-C006 AsyncOutboundApi | `solution-go-messaging-infrastructure` + `solution-go-kafka-producer` *(skeleton)* | — |
| VP-C010 OutboxPattern | `solution-go-transactional-outbox` *(skeleton)* | requires VP-C006 AND VP-C001 ≠ None |
| VP-C007 AsyncInboundApi | `solution-go-messaging-infrastructure` + `solution-go-kafka-consumer` *(skeleton)* | — |
| VP-C002 TransientStore `Redis` | `solution-cached-db` (`depends_on` `solution-go-domain-ports`) | — |
| VP-C001 PersistentStore `PostgreSQL` | `solution-persistent-db` (`depends_on` `solution-go-domain-ports`), optionally + `solution-go-db-migrations` (`depends_on` `solution-persistent-db`; not composed by any of the 5 plateaus yet) | — |

## 4. Link & path conventions

- Every internal link points inside `skills/go/architecture/` — this catalog has no version-prefixed staging tree (no pre-existing catalog to parallel-build against; see `agent/DECISIONS.md`).
- **Carve-out:** `solution-conformance-testing-in-go` legitimately `depends_on`/references `skills/testing/core/solution-conformance-testing.skill` (the stack-agnostic parent it implements, including its own `adr/mutation-tool-per-stack.md`, which this catalog's build updated directly) and `skills/testing/go/cucumber-testing-in-go.skill.md` (the scenario-authoring rules it delegates to). `solution-go-repository-structure`, `solution-go-domain-logic`, `solution-go-http-api`, and every `Package.create.md`/`Struct.template`/`Functions.template` also legitimately reference `skills/design/skill-design.skill/skill-design.skill.md` and `skills/common-workflow/architecture/design/*` (the pipeline skills themselves). `solution-go-db-migrations` legitimately references (body prose only, never `depends_on:` — it is not a `solution-*.skill.md`) `skills/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md`, whose own "migration step" rule owns the deployment topology that gates `cmd/migrate` ahead of the app on each platform. `solution-go-http-api`, `solution-grpc-api`, `solution-external-integration`, and the plateau skills built from them legitimately reference `skills/common-workflow/develop/api-develop.skill`, `api-develop-in-go.skill`, and `api-client-develop.skill.md` — the owners of the contract rules ([§9](#9-api-contracts-issue-206)). These are the only allowed external `depends_on`/references in the catalog.
- Wikilink form: `[[skills/go/architecture/solutions/solution-x.skill/solution-x.skill.md|solution-x]]`. Frontmatter `depends_on` entries end with `.skill.md` before the `|`.
- Implementation-file links: `[[.../solution-x.skill/Implementation/{path}/{File}.{kind}.md#SECTION|label]]`.
- A solution's folder name, its main file name, and its `name:` field are identical: `solution-{name}.skill` / `solution-{name}.skill.md` / `name: solution-{name}`.
- Placeholder directories/files (`{service}`, `{adapter}`, `{cache}`, `{store}`) are literal folder/file names inside `Implementation/`, matching this repository's existing dotnet/python convention (`{Module}`, `{App}`) — never resolved to a concrete name inside `solutions/`, only inside a plateau's `examples/`.

## 5. Frontmatter policy

- `version:` — `20260917000000` for every solution created in this build (bump only on a later real edit).
- `built_on_plateau:` — **empty** for every solution until this catalog's plateaus exist (they are created after the catalog). State the assumed baseline in `# Boundaries` prose instead.
- `depends_on:` — only real structural dependencies, each resolving to a `solution-*.skill.md` inside `solutions/`. A whole-plateau assumption is a `# Boundaries` note, not a `depends_on`.
- `tags:` — per skill-tags: `skill/architecture/solution`, `solution/{name}`, one `stack/go`, ≥1 `concern/*`. Implementation files: `solution/{name}` + `element/{name}`. ADRs: `solution/{name}` + `concern/documentation` + `concern/documentation/adr` + `stack/go`.

## 6. skill-design compliance (current)

- No `## MUST NOT` / `## SHOULD NOT` headings and no `# Anti-patterns` section anywhere. Every prohibition is a negatively-phrased bullet under `## MUST`/`## SHOULD`.
- Every `## MUST` bullet that states a rule carries `Risk:` + `Fix:` (`Violation:` optional).
- Exactly one `# Goal`, one `# Core Principle`(s), one `# Rule`(s), one `# Check list` per skill file.

## 7. Per-classification change checklist

**new (every solution in this build so far):** authored via `solution-create`'s `templates/go/`; ADR + glossary as needed; `Implementation/` covers every created/extended file with concrete Go code, never left as prose-only.

**skeleton (aspirational — VP-C006/VP-C010/VP-C007's four solutions):** main skill file complete (Goal, Core Principle, Boundaries where real, Rule, Check list) + a `> Draft contract — no consumer yet` marker + at least one shape-only Implementation file. Full authoring (a grounded Kafka client choice, an AS-IS/TO-BE extension of `{service}.go`, a real ground-truth example) deferred until a sixth plateau consumes them.

## 8. Ground truth

Each of the five plateaus' own `examples/` must `go build ./...`, `go vet ./...`, and `make test-kind-unit` (godog scenarios green) — `plateau-persistent-service`'s example additionally needs a reachable PostgreSQL and Redis to run its full scenario set; document the exact `docker run`/connection-string setup in that plateau's own root skill. Until a plateau's example passes, that plateau is "plausible", not "verified".

## 9. API contracts (issue #206)

The catalog follows `api-develop` and `api-develop-in-go`. Those two skills own the rules; this section fixes only what the catalog adds — which solution owns which file, and what is the same in every plateau. Owner decisions of 2026-10-12 are marked **(owner)**.

### Who owns what

| Solution | Owns |
| --- | --- |
| `solution-go-http-api` (baseline) | `docs/integration/api/{contract-name}/openapi.json` hand-written **(owner)** and `embed.go`; `internal/api/http/` — `server.go`, `docs.go`, `errors.go`, the feature and its steps; `HTTP_API_DOCS_ENABLED` and `HTTP_BASE_PATH` in `config.go` |
| `solution-grpc-api` (VP1) | replaces the hand-written spec by `{contract-name}/{file}.proto` and the `{file}.swagger.json` generated from it; `buf.yaml`, `buf.lock`, `buf.gen.yaml`, the `tool` lines of `go.mod`, `make proto-gen`, `gen/api/{contract-name}/`, `tools/testing/kinds/generated.sh`; rewrites the HTTP adapter onto the generated messages (`protojson.go` replaces `errors.go`); `internal/api/grpc/` |
| `solution-external-integration` (VP-C005) | `docs/integration/client/{api-name}/{file}.proto`, the provider's copy unchanged; the second module of `buf.yaml`; `buf.gen.client.yaml`, which sets `go_package` by buf managed mode; `gen/client/{api-name}/` |

### The same in every plateau

- **One wire form.** `POST /v1/…` routes; JSON keys in lowerCamelCase; every field present in a response; an unknown request field ignored; an unparseable body answered 400. Error body `{"code", "message", "details"}` — `google.rpc.Status` — with the status from the canonical gRPC-to-HTTP table (`ErrUnavailable` → 503, not 502). Success status 201 / 202 / 200 by what the method did. The baseline writes this by hand with `encoding/json`; from VP1 on it is `protojson` over the generated messages (handlers written by hand, no grpc-gateway — **owner**). The same feature file passes on both, so adding VP1 changes no byte a client sees.
- **Contract.** One folder per contract. The proto file starts with `// version: {major}.{minor}.{patch}`, declares `package {name}.v{major};` and `go_package "{module-path}/gen/api/{contract-name}"`; no version folder. The spec's `info.version` equals it.
- **Docs routes.** `GET /openapi.json`, `GET /swagger/`, and `GET /swagger` redirecting to it — registered only when `HTTP_API_DOCS_ENABLED=true`. Routes stay mounted at `/`; `HTTP_BASE_PATH` goes only into emitted URLs: the spec's `servers[0].url` (OpenAPI 3, hand-written) or `basePath` (OpenAPI 2, generated), Swagger UI's spec and asset URLs, the redirect's `Location`.
- **Swagger UI.** `github.com/swaggest/swgui` `v5emb`, pinned in `go.mod` — assets embedded in the binary through a Go module **(owner)**.
- **Routes.** Contract routes come from one `contractRoutes()` list exposed as `ContractRoutes()`; `/health` and the docs routes are outside it.
- **Feature.** `internal/api/http/features/http_api.feature`, `@type/api/http`: the operations scenario of `api-develop-in-go`; each method's success status and body; a request with an unknown field; an invalid and an unparseable request; the docs routes on, under `/prefix`, and off.
- **Generators.** `tool` lines of `go.mod`, run as `go tool …`; nothing installed with `@latest`.

### Not in this change

- `/health/live` and `/health/ready` of `devops-service-deploy`: the catalog keeps `GET /health` only. A readiness that checks Redis, PostgreSQL and the outbound grpc client is a change of the adapters and their ports, not of the API contract — a separate issue.
- The skeleton solutions (Kafka, outbox) and `solution-go-db-migrations` are untouched.

### Ground truth, added to §8

Every plateau example passes `make test-and-report`; one with a proto contract lists the `generated` kind and passes it. Each binary was started with `HTTP_API_DOCS_ENABLED=true HTTP_BASE_PATH=/prefix` and answered `/openapi.json`, `/swagger/`, and a Swagger UI asset. `check.sh` §10 holds the mechanical part: no statement of the previous layout or wire form, and in every example the contract folder, the docs switch, the operations scenario, and — with a proto contract — `buf.yaml`, `buf.lock`, the `tool` lines, and the `generated` kind.
