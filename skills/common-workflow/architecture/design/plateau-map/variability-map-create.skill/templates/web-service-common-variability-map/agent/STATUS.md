# Status

Worktree `.ai-worktree/common-variability-map`, branch `common-variability-map` (base `develop`).

| Step | Content | State |
| --- | --- | --- |
| W0 | First anchor (full 16-VP list) | superseded — owner chose rebuild-from-zero |
| W1 | Framework: empty inherited common map, rules + ADR, two-table template, `check.sh` | done `86fa7add` |
| A | VP statuses (💡/📐/⛔, ⏳/✅) + staged admission; candidates moved into the common map; Storage 📐 (VP-C001 PersistentStore, VP-C002 TransientStore), ⏳ rows in go/dotnet | done |
| B | `plateau-map-create`: plateau codes, letter registry, shared common-plateau registry (empty), plateau statuses, matrix Code/Title columns, ADR `plateau-code-by-combination`; `plateau-create-by-solutions` takes the code as `{plateau-name}` | done |
| C | Storage ✅ in Go: rows detailed, VP7→VP-C001, VP6→VP-C002 re-ID, plateau-repository recoded (GW001.000 … GW003.002), registry rows 001–003 | done |
| D | Storage ✅ in dotnet: rows detailed (VP-C001 Refined: requires VP1), VP2→VP-C001 re-ID (46 files), plateau-repository recoded (DW001.000, DW004.001, DW004.002), registry row 004 | done |
| E | TaskBox 📐 (VP-C003) + TransientStore concept sharpened (lifetime); ⏳ rows in go/dotnet; registry + matrices get the VP-C003 column; feature template: TaskBox own feature, criticality on TaskBox | done |
| F | TaskBox storage contract `contracts/vp-c003-taskbox.md` — two review rounds, accepted | done |
| G | TaskBox ✅ in go/dotnet: own realization of the contract per store, clients chosen | done |
| H | Outbound protocols: VP-C004 HttpOutbound, VP-C005 GrpcOutbound 📐 + ✅ in go/dotnet; Go VP2, dotnet VP10/VP11 re-IDed; plateaus recoded; registry 005–008, 002–004 retired | done |
| I | Messaging 📐: VP-C006–VP-C009 (Kafka/RabbitMQ producer/consumer), CloudEvents; ⏳ rows in go/dotnet; registry + matrices get 4 columns (all No, no recode); Inbox candidate | done |
| J | Messaging ✅ in go (franz-go, amqp091-go) and dotnet (Confluent.Kafka, RabbitMQ.Client), CloudEvents; Go VP3/VP5, dotnet VP12/VP13 re-IDed | done |
| K | Outbox VP-C010 📐 + ✅ (envelope contract; TaskBox v1 amended: status-code outcomes, `last_status`); Go VP4, dotnet VP14 re-IDed; registry value shift fixed + registry↔matrix cross-check in check.sh | done |
| L | Inbox VP-C011 📐 + ✅ (only-once mode for broker messages and calls; contract; `status_key` in TaskBox v1) | done |
| M | Saga dropped (owner): TaskBox/Inbox already are the dispatch mechanism; multi-service sagas go over the brokers | done |
| N | Common map split: concepts → `vp/vp-c###-{name}/vp-c###-{name}.md`, contracts beside them as `.contract.md`, candidates → `candidates.md`; ID cells and stack maps link the concept file; ADR `common-vp-concept-per-file`; `check.sh` checks concept files both ways | done |
| O | Implementation plan TaskBox → Outbox recorded below | done |
| P | TaskBox in Go (plan steps 1–3) — worktree `taskbox-go`; anchor `agent/taskbox/INVARIANTS.md`, owner answers applied to the contract; W1–W4 done (PostgreSQL), W5 Redis next | in progress |
| Q | Contract VPs as libraries (owner, 2026-09-29): solution-create ADR + rule; TaskBox repositories prepared locally in `tmp/vp-c003-taskbox/` (`taskbox-spec` with the contract and feature; `taskbox-go` / `-dotnet` / `-python` empty with TASK.md). **Pending:** ai-skills side not migrated yet — the contract still lives here too (duplicate of `taskbox-spec`), `solution-taskbox-in-go` still carries the code, GW009.001's example still has `internal/taskbox` | in progress |
| next | **Owner validation of the branch.** Remaining candidates afterwards: Inbound protocols, DomainLogic, Metric, Domain modelling, Deployment | waiting on owner |

## Implementation plan — TaskBox → Outbox (agreed 2026-09-28, a separate task)

The common map is designed; nothing realizes TaskBox or Outbox yet. Order, Go first so the contracts are proven by code before dotnet repeats them:

| # | Step | Result |
| --- | --- | --- |
| 1 | Common solution `solution-taskbox` — stack-agnostic, realizes VP-C003 by the contract `vp/vp-c003-taskbox/vp-c003-taskbox.contract.md` (location per the `solution-conformance-testing` precedent in `skills/common-workflow/`) | the shared rules + conformance scenarios every stack solution follows — **done**: `skills/common-workflow/architecture/solutions/solution-taskbox.skill` |
| 2 | `solution-taskbox-in-go` — PostgreSQL first (`pgx`), then SQLite / Redis / InMemory | Go realization; VP-C003 `planned` → solution link — **PostgreSQL done** (VP-C003 PostgreSQL linked); Redis next (W5); SQLite / InMemory stay `planned` (owner, Q3) |
| 3 | Plateau "existing Go base + TaskBox" (base: `GW007.001`, the only one with PostgreSQL), named by its code | runnable example passing the TaskBox conformance scenarios — **done**: `GW009.001` (`skills/go/architecture/plateau/gw009-001`), registry row 009 |
| 4 | Common solution `solution-transactional-outbox` — realizes VP-C010 by `vp/vp-c010-outbox/vp-c010-outbox.contract.md` | shared rules + scenarios |
| 5 | `solution-transactional-outbox-in-go` — replaces the Kafka-only skeleton `solution-go-transactional-outbox`; generic `outbox.http` / `outbox.kafka` / `outbox.rabbitmq` handlers | Go realization; VP-C010 → solution link |
| 6 | Plateau "step 3 + Outbox" | runnable example passing the Outbox scenarios |
| 7 | Same for dotnet: `solution-taskbox-in-dotnet`, `solution-transactional-outbox-in-dotnet` (the existing `solution-transactional-outbox` in dotnet is renamed — the bare name now belongs to the common solution), plateaus on `DW008.00x` | |

**Naming (owner, 2026-09-28):** a common (stack-agnostic) solution is `solution-{name}`; its stack realization is `solution-{name}-in-{stack}`. Existing stack solutions that break this (`solution-go-*`, dotnet's bare names) are renamed when they are next touched.

## Follow-ups (outside this PR)

- **Align outbox skeletons to VP-C010:** Go `solution-go-transactional-outbox`, dotnet `solution-transactional-outbox` — drop their own Kafka-only outbox tables; become the TaskBox-based envelope with generic HTTP/Kafka/RabbitMQ handlers.

- **Align messaging skeletons to VP-C006–VP-C009:** Go and dotnet messaging infrastructure → CloudEvents envelope; Go client → franz-go with the own Kafka binary binding.

- **Align outbound solutions to VP-C004/VP-C005:** Go `solution-external-integration` (failures → HTTP status codes); dotnet `solution-http-api-client` / `solution-grpc-client` (domain-named port instead of `I{Dependency}Client`; failures → HTTP status codes).

- **Rename existing plateau folders/files to their codes** (Go 5, dotnet 3 plateaus; dotnet `structure/` file names embed the plateau name). Wanted as a GitHub issue — `gh` is not authenticated in this environment; issue text handed to the owner. Best done after the candidates covering existing stack VPs are admitted, so codes stop changing.
- `plateau-map-create`'s "stop if Realized by has gaps" must treat `planned — …` as filled (handled in B).
