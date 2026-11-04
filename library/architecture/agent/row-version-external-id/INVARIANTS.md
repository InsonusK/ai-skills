# Row version and external id — invariants

The anchor document for the two kinds a table may have beside `vp-postgres`: rows that change (`vp-row-version`) and rows whose identity the caller sets (`vp-external-id`), in Go, Python, and dotnet (per [[library/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Owner's decisions of 2026-11-04 are folded in and logged in `DECISIONS.md`; the plan is in `STATUS.md`.

## 1. Two axes per table

Every table answers two questions; each answer is one variation point, attached on its own.

| | Rows change: `vp-row-version` | Identity set by the caller: `vp-external-id` |
|---|---|---|
| Row, migration | a version: PostgreSQL's `xmin`, no column | PK `uuid`, no default; a service-set PK stays `bigint` identity |
| Repository | `Update`, `Delete` compare the version; no row is `RowChanged`; the new version is put back on the row | `Insert` saves the caller's id; a taken id is `DuplicateKey` |
| Service | `RowChanged` → `concurrent change` | `DuplicateKey` of the id → `conflict` |
| API | the version in every read a caller may edit from and in the answer of create and update; a change with no version → `invalid` | the id in the create request; anything but a UUID v7 → `invalid` |

- The four combinations are the table of `vp-postgres` (Classify every table). Neither variation point knows of the other.
- Where they meet: a row the caller created and may change answers its version on create, as the caller already has its id and needs the version for the first edit.

## 2. The version

- It is `xmin`, kept by the engine; no code sets it.
- It is opaque: a `string` in every contract (proto `string version`, JSON `"version"`), compared for equality only, so a later engine (SQLite) keeps the contract.
- The base repository of `vp-postgres` compares it when the row declares one, in every stack: `Update` and `Delete` put it into `WHERE` and leave it out of `SET`, and read the new one back (`RETURNING xmin`). A row with no version is written as today. This is a change of `vp-postgres`'s fixed files, the one this pair makes; `vp-row-version` adds only the declaration on a row, the services, and the API.
- `Update(change)` of a selector compares the version of every row it saves; one moved row fails the whole call.

## 3. The external id

- A UUID v7, made by the caller: its first 48 bits are the time, so inserts go to the end of the index (measured on PostgreSQL 17.5: 5 million rows in 25.5 s and 438 MB against 38.4 s and 631 MB for a `bigint` PK beside a random guid).
- The entry point refuses an id that is no UUID v7 (version field 7, RFC 9562 variant) with `invalid`.
- It is the PK and every FK to the row is `uuid`; the row has no second id.
- A UUID v7 tells when the row was created. An agent that sees the creation time may be secret (who did what when, an order of events not to disclose) warns the user before choosing it.
- A taken id is `conflict` ("{thing} {id} already exists"). A repeated request is told apart by `vp-idempotency` (later, §5), never here.

## 4. Marks

- Every mapped row declares both axes: it changes or never does; its identity is set by the service or by the caller.
- A check over the mapped rows fails on a row with a mark missing, in every stack (a scenario of the shared conformance).

## 5. Scope and order

| Wave | What |
|---|---|
| 0 | the spec of both (`spec/vp-row-version`, `spec/vp-external-id`), their `conformance/`, the classification of `vp-postgres` updated |
| 1 | `vp-row-version-in-{go,python,dotnet}`: the base repository's version check, the declaration, a service and an API over a changing table |
| 2 | `vp-external-id-in-{go,python,dotnet}`: the uuid PK, the v7 check at the entry, `conflict` on a taken id |
| later | `vp-idempotency`: header `Idempotency-Key` (gRPC metadata `idempotency-key`), a table of keys with the stored answer, in front of the command; a request whose key is known gets the stored answer and the command does not run |

- Each wave is its own PR; wave 0 is #280.
- The shared scenarios run on test tables the application never ships: `notes` (service id, changes) and `drafts` (caller id, changes).

## 6. Done when

- Every stack runs both `conformance/` folders unedited; `test/test.sh` and `library/testing/agent/check.sh` pass.
- Every example in scope passes its tests and its type check; the DB ones on PostgreSQL.
