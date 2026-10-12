# Task: bring the Go catalog to api-develop (issue #206, the rest of #201)

Status document per `bulk-authoring-harness`. The contract is [INVARIANTS.md §9](./INVARIANTS.md#9-api-contracts-issue-206).

## Waves

| Wave | Artifacts | Kind | Status |
| --- | --- | --- | --- |
| W0 | `examples/` of `plateau-http-service` and `plateau-dual-api-service` — the two reference implementations the anchor was written from | modify | done, `make test-and-report` green |
| W1 | `api-develop-in-go`: the hand-written spec of a service with no proto contract, `inputs` in `buf.gen.yaml`, where `HTTP_BASE_PATH` goes in a v2 and a v3 spec, the Swagger UI module | modify | done |
| W1b | `agent/check.sh`: the mechanical part of §9 | modify | — |
| W2 | `solution-go-http-api` (through `solution-update`, ADRs) and `plateau-http-service`: root skill, `structure/` skills | modify | — |
| W3 | `solution-grpc-api` and `plateau-dual-api-service`: root skill, `structure/` skills | modify | — |
| W4 | `solution-external-integration` and `plateau-integrated-service`: skills and example | modify | — |
| W5 | `solution-persistent-db` (`server.go.extend`), `plateau-cached-service`, `plateau-persistent-service`: skills and examples | modify | — |
| W6 | `gw009-001`: skills and example; `registry/`, `plateau/plateau-repository.md`, `feature/` where they name a moved path | modify | — |

Each wave: `bash skills/go/architecture/agent/check.sh`, the example's `make test-and-report`, a fresh-eyes audit against §9 and the two API skills, one commit.

## Done when
- The "Done when" list of issue #206 holds.
- `check.sh` fails on `proto/{service}/`, `buf/buf.gen.yaml`, a generator installed with `@latest`, and an `{"error": …}` body anywhere in the catalog.
