# Row version and external id — status

| Wave | What | State |
|---|---|---|
| 0 | spec of both, `conformance/`, `vp-postgres`'s classification | done; `INVARIANTS.md` agreed by the owner; specs merged in #280 |
| 1 | `vp-row-version-in-{go,python,dotnet}` | implemented and validated; integration in progress |
| 2 | `vp-external-id-in-{go,python,dotnet}` | — |
| later | `vp-idempotency` | — |

## Completion on the entities base — 2026-11-14

The owner confirmed integration of the entities chain and completion of row version.
External id and subsequent VPs retain their own implementation stages.

| Wave | Artifacts / classification | State |
|---|---|---|
| I | #294 → arch-db-entities; #300 → arch-new-vp | merged |
| R1 | Go/Python PostgreSQL version checks and UoW copies (copy-and-modify from #292) | done; audited; Go tests/vet and Python 240/272 tests + mypy pass |
| R2 | HTTP contract scenario and per-example operation pins (owner selected separation) | done; audited; all nine consumers pass |
| R3 | Go row-version skill/example/scenarios (copy-and-modify on current PostgreSQL base) | done; audited; full runtime and static checks pass |
| R4 | Python row-version skill/example/scenarios (copy-and-modify on current PostgreSQL base) | done; audited; full runtime and static checks pass |
| R5 | Gherkin vocabulary, README and historical status entries, validation gate (modify/new) | audited; 1085 feature metadata checks and Go/Python baseline runtime/type checks pass; catalog sync and testing checks pass |
| R6 | successor PR → arch-new-vp → develop → master; close superseded #292 | planned |

Current dotnet row version is already delivered by #294 and is preserved;
Go/Python keep their agreed HTTP entry points. Each wave runs the mechanical
checks and the affected examples on PostgreSQL, receives a fresh-eyes audit and
is committed separately. The existing INVARIANTS.md remains the row-version
contract, with the dotnet entities form defined by dotnet-entities/INVARIANTS.md.

### Validation checkpoint — 2026-11-15

- The owner approved separate shared HTTP/OpenAPI equality and native operation
  pins. All nine consumers carry both checks, including CompleteItem in the new
  Go/Python examples. The gate rejects a missing native pin.
- Full Go HTTP-proto/PostgreSQL/UoW/row-version suites and go vet pass.
- Full Python HTTP-proto/PostgreSQL/UoW/row-version suites pass:
  161/241/273/281 tests; mypy is clean in all four examples.
- The dotnet HTTP-proto suite passes 160 tests. The four preserved CLI examples
  pass 93/175/198/200 tests (666 total), without failures or skips.
- Structural checks pass: 53 VP diagrams, 242 template pairs, 851 conformance
  copies, 1094 feature metadata checks and nine native HTTP operation pins.
- R3/R4 exercise successful completion, an independent raw UPDATE, and unusable
  caller versions. Nonnumeric/negative/out-of-range values now reject the write
  as concurrent change, matching dotnet, rather than returning a server error.
- R5 is committed as c62e8fb2. R2/R3/R4 are validated, audited and committed;
  integration is being completed. #292 remains open until #301 replaces it.
