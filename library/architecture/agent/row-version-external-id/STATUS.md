# Row version and external id — status

| Wave | What | State |
|---|---|---|
| 0 | spec of both, `conformance/`, `vp-postgres`'s classification | done; `INVARIANTS.md` agreed by the owner; specs merged in #280 |
| 1 | `vp-row-version-in-{go,python,dotnet}` | — |
| 2 | `vp-external-id-in-{go,python,dotnet}` | — |
| later | `vp-idempotency` | — |

## Completion on the entities base — 2026-11-14

The owner confirmed integration of the entities chain and completion of row version.
External id and subsequent VPs retain their own implementation stages.

| Wave | Artifacts / classification | State |
|---|---|---|
| I | #294 → arch-db-entities; #300 → arch-new-vp | merged |
| R1 | Go/Python PostgreSQL version checks and UoW copies (copy-and-modify from #292) | done; audited; Go tests/vet and Python 240/272 tests + mypy pass |
| R2 | HTTP contract scenario and per-example operation pins (decision pending) | waiting for owner |
| R3 | Go row-version skill/example/scenarios (copy-and-modify on current PostgreSQL base) | code and focused runtime checks audited; HTTP contract choice pending |
| R4 | Python row-version skill/example/scenarios (copy-and-modify on current PostgreSQL base) | code and focused runtime checks audited; HTTP contract choice pending |
| R5 | Gherkin vocabulary, README and historical status entries, validation gate (modify/new) | audited; 1085 feature metadata checks and Go/Python baseline runtime/type checks pass; catalog sync and testing checks pass |
| R6 | successor PR → arch-new-vp → develop → master; close superseded #292 | planned |

Current dotnet row version is already delivered by #294 and is preserved;
Go/Python keep their agreed HTTP entry points. Each wave runs the mechanical
checks and the affected examples on PostgreSQL, receives a fresh-eyes audit and
is committed separately. The existing INVARIANTS.md remains the row-version
contract, with the dotnet entities form defined by dotnet-entities/INVARIANTS.md.
