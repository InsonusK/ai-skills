# skills/go/architecture catalog invariants

The anchor document for this catalog's build (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Every artifact in `solutions/` and `plateau/` must satisfy every invariant here. `check.sh` enforces the mechanical ones; the per-wave audit enforces the rest.

## 1. Baseline (what exists before any optional feature)

```
go.mod
Makefile                          ← solution-go-repository-structure (build/run/lint) +
                                     solution-go-conformance-testing (unit-test/mutation-test/
                                     test-report/test-and-report)
.gitignore
report-template/index.html        ← solution-go-conformance-testing
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
      server.go                   ← solution-go-http-api
tools/
  normalize_unittest/main.go      ← solution-go-conformance-testing
  normalize_mutation/main.go      ← solution-go-conformance-testing
  test_report/main.go             ← solution-go-conformance-testing
```

- **No `internal/domain/interfaces/`** at baseline — created by `solution-go-domain-ports`, a shared prerequisite the first-applied of `solution-external-integration`/`solution-cached-db`/`solution-persistent-db` depends on (VP2/VP6/VP7).
- **No `internal/infrastructure/`, `internal/api/grpc/`, `proto/`, `buf/`, `gen/`** at baseline — each created by its own VP-realizing solution.
- Every Go module dependency is pinned once in `go.mod` — no separate central-package-management solution exists for this stack (unlike the dotnet catalog); see `feature/feature-model.md`'s "Modeling choices" section.

## 2. Common features → realizing solution (must be 1:1 covered)

| Feature (feature-model.md) | Solution |
| --- | --- |
| (baseline repo/composition-root structure) | `solution-go-repository-structure` |
| (shared outbound-ports package prerequisite) | `solution-go-domain-ports` — not a Feature Model row itself, a shared prerequisite of VP2/VP6/VP7 |
| DomainLogic | `solution-go-domain-logic` |
| HttpApi | `solution-go-http-api` |
| AppLogging | `solution-go-app-logging` |
| TestConformance (+ Cucumber / Code Coverage / Mutation / Test Reports) | `solution-go-conformance-testing` |

## 3. Variation Points → realizing solution(s) (must be 1:1 covered)

| VP | Solution(s) | Constraint (from variability-map.md) |
| --- | --- | --- |
| VP1 GrpcApi | `solution-grpc-api` | — |
| VP2 ExternalIntegration | `solution-external-integration` (`depends_on` `solution-go-domain-ports`) | — |
| VP3 AsyncOutboundApi | `solution-go-messaging-infrastructure` + `solution-go-kafka-producer` *(skeleton)* | — |
| VP4 OutboxPattern | `solution-go-transactional-outbox` *(skeleton)* | requires VP3 AND VP7 |
| VP5 AsyncInboundApi | `solution-go-messaging-infrastructure` + `solution-go-kafka-consumer` *(skeleton)* | — |
| VP6 CachedDb | `solution-cached-db` (`depends_on` `solution-go-domain-ports`) | — |
| VP7 PersistentDb | `solution-persistent-db` (`depends_on` `solution-go-domain-ports`), optionally + `solution-go-db-migrations` (`depends_on` `solution-persistent-db`; not composed by any of the 5 plateaus yet) | — |

## 4. Link & path conventions

- Every internal link points inside `skills/go/architecture/` — this catalog has no version-prefixed staging tree (no pre-existing catalog to parallel-build against; see `agent/DECISIONS.md`).
- **Carve-out:** `solution-go-conformance-testing` legitimately `depends_on`/references `skills/common-workflow/test/solution-conformance-testing.skill` (the stack-agnostic parent it implements, including its own `adr/mutation-tool-per-stack.md`, which this catalog's build updated directly) and `skills/go/testing/cucmber-testing-in-go.skill.md` (the scenario-authoring rules it delegates to). `solution-go-repository-structure`, `solution-go-domain-logic`, `solution-go-http-api`, and every `Package.create.md`/`Struct.template`/`Functions.template` also legitimately reference `skills/design/skill-design.skill/skill-design.skill.md` and `skills/common-workflow/architecture/design/*` (the pipeline skills themselves). `solution-go-db-migrations` legitimately references (body prose only, never `depends_on:` — it is not a `solution-*.skill.md`) `skills/devops/devops-service-deploy.skill/devops-service-deploy.skill.md`, whose own "migration step" rule owns the deployment topology that gates `cmd/migrate` ahead of the app on each platform. These are the only allowed external `depends_on`/references in the catalog.
- Wikilink form: `[[skills/go/architecture/solutions/solution-x.skill/solution-x.skill.md|solution-x]]`. Frontmatter `depends_on` entries end with `.skill.md` before the `|`.
- Implementation-file links: `[[.../solution-x.skill/Implementation/{path}/{File}.{kind}.md#SECTION|label]]`.
- A solution's folder name, its main file name, and its `name:` field are identical: `solution-{name}.skill` / `solution-{name}.skill.md` / `name: solution-{name}`.
- Placeholder directories/files (`{service}`, `{adapter}`, `{cache}`, `{store}`) are literal folder/file names inside `Implementation/`, matching this repository's existing dotnet/python convention (`{Module}`, `{App}`) — never resolved to a concrete name inside `solutions/`, only inside a plateau's `example/`.

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

**skeleton (aspirational — VP3/VP4/VP5's four solutions):** main skill file complete (Goal, Core Principle, Boundaries where real, Rule, Check list) + a `> Draft contract — no consumer yet` marker + at least one shape-only Implementation file. Full authoring (a grounded Kafka client choice, an AS-IS/TO-BE extension of `{service}.go`, a real ground-truth example) deferred until a sixth plateau consumes them.

## 8. Ground truth

Each of the five plateaus' own `example/` must `go build ./...`, `go vet ./...`, and `make unit-test` (godog scenarios green) — `plateau-persistent-service`'s example additionally needs a reachable PostgreSQL and Redis to run its full scenario set; document the exact `docker run`/connection-string setup in that plateau's own root skill. Until a plateau's example passes, that plateau is "plausible", not "verified".
