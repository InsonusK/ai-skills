---
name: plateau-repository.example
description: A worked plateau-repository.md for a small fictional dotnet "notification-service" catalog — 1 common and 4 stack Variation Points, 3 plateaus — showing plateau codes, the matrix, legend, stack-VP combinations, lineage, and shape notes this skill produces
tags:
  - stack
  - concern/architecture
---

# Worked example: `notification-service/plateau/plateau-repository.md`

This is the file `plateau-map-create` produces for a fictional catalog at
`skills/example/architecture/notification-service/`, a dotnet backend web-service
family (code prefix `DW`). Its Variability Map (`notification-service/variability-map.md`)
carries one 📐 common VP — `VP-C001` PersistentStore, `None / PostgreSQL / SQLite`,
detailed ✅ with PostgreSQL realized by `solution-persistence` — and 4 stack VPs, every
column filled:

| VP | Variants | Constraint | Realized by |
|----|----------|------------|-------------|
| VP1 Channel | `email` \| `sms` \| `push` (alternative) | — | `solution-email-channel`, `solution-sms-channel`, `solution-push-channel` |
| VP2 Templating | on \| off (boolean) | requires VP1 | `solution-mustache-templates` |
| VP3 DeliveryReceipts | on \| off (boolean) | requires VP1 | `solution-receipt-tracking` |
| VP4 RetryPolicy | `none` \| `fixed` \| `exponential` (alternative) | — | `solution-fixed-retry`, `solution-exponential-retry` |

Three plateaus exist on disk, built by `plateau-create-by-solutions` and named by
the file form of their codes: `dw001-000` (baseline, `standalone: false`),
`dw002-001` (`parent: dw001-000`), `dw002-002` (`parent: dw002-001`). The shared
common-plateau registry holds `001` = PersistentStore `None` and `002` =
PersistentStore `PostgreSQL`.

---

## Plateau × VP matrix

Rows = plateaus by code, with the Title decoding it; columns = the common VP (cell = the
Variant the plateau realizes) and the 4 stack VPs (✅ = realized at that plateau,
❌ = not). Answers are **cumulative** down the chain — a plateau has
every VP its parent has, plus its own. Scan a **column** for the shallowest plateau
that includes a VP; read a **row** for a plateau's complete VP set.

| Code | Title | VP-C001 | VP1 | VP2 | VP3 | VP4 |
|---|---|:-:|:-:|:-:|:-:|:-:|
| DW001.000 | core          | None       | ❌ | ❌ | ❌ | ❌ |
| DW002.001 | transactional | PostgreSQL | ✅ | ❌ | ✅ | ✅ |
| DW002.002 | marketing     | PostgreSQL | ✅ | ✅ | ✅ | ✅ |

Column legend — VP-C001 PersistentStore (common) · VP1 Channel · VP2 Templating ·
VP3 DeliveryReceipts · VP4 RetryPolicy.
Full descriptions, the solution that realizes each VP, and the constraints between VPs
are in the catalog's `../variability-map.md` — the single source of truth; this table
is only the plateau-oriented view of the same answers.

- **VP1 is decided per message type** — a ✅ means the plateau *composes* at least one
  channel solution and its example demonstrates it, not that every message type uses
  every channel.
- **VP4 `none` is not a ❌** — `DW001.000` has no retry policy at all (❌);
  `DW002.001` composes `solution-fixed-retry`, so VP4 is ✅ there even
  though a caller may still select the `none` variant per message type.

## Stack VP combinations

| `{specific}` | Stack VPs |
|---|---|
| 000 | — |
| 001 | VP1, VP3, VP4 |
| 002 | VP1, VP2, VP3, VP4 |

A fourth plateau with PersistentStore `SQLite` and the same stack VPs as
`DW002.001` would be `DW{n}.001`, where `{n}` is the registry's number for
PersistentStore `SQLite` — the stack part is reused, not renumbered.

## Lineage & new solutions

| # | Plateau | `standalone` | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------------|--------|---------------------------------------------------------------|
| 1 | **DW001.000** core | `false` | — | `solution-message-envelope`, `solution-dispatch-pipeline`, `solution-logging` |
| 2 | **DW002.001** transactional | `true` | DW001.000 | VP-C001 `solution-persistence` · VP1 `solution-email-channel` + `solution-sms-channel` · VP3 `solution-receipt-tracking` · VP4 `solution-fixed-retry` |
| 3 | **DW002.002** marketing | `true` | DW002.001 | VP1 `solution-push-channel` · VP2 `solution-mustache-templates` · VP4 `solution-exponential-retry` (replaces `solution-fixed-retry` via DI re-registration — `TD-`, recorded in `registry/retrypolicy-cs`) |

## Constraint check

- VP2 `requires VP1`: `DW002.002` sets VP2 ✅ and VP1 ✅ — consistent.
- VP3 `requires VP1`: `DW002.001` sets VP3 ✅ and VP1 ✅ — consistent.
- No plateau sets VP2 or VP3 without VP1. No violations.

## Combinations the family allows that no plateau covers yet

`VP1 ✅` + `VP2 ✅` + `VP3 ❌` — templated messages with no delivery receipts — is a
legal combination with no dedicated plateau, because `DW002.002` (the only
plateau with VP2) inherits VP3 from `DW002.001`. Future work if a caller
needs it.
