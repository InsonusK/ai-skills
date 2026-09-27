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
| next | Messaging ✅ per stack (library proposal to the owner), then Outbox → Inbox → Saga | waiting on owner |

## Follow-ups (outside this PR)

- **Align outbound solutions to VP-C004/VP-C005:** Go `solution-external-integration` (failures → HTTP status codes); dotnet `solution-http-api-client` / `solution-grpc-client` (domain-named port instead of `I{Dependency}Client`; failures → HTTP status codes).

- **Rename existing plateau folders/files to their codes** (Go 5, dotnet 3 plateaus; dotnet `structure/` file names embed the plateau name). Wanted as a GitHub issue — `gh` is not authenticated in this environment; issue text handed to the owner. Best done after the candidates covering existing stack VPs are admitted, so codes stop changing.
- `plateau-map-create`'s "stop if Realized by has gaps" must treat `planned — …` as filled (handled in B).
