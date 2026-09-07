---
name: plateau-repository.example
description: A worked plateau-repository.md for a small fictional "notification-service" catalog — 4 Variation Points, 3 plateaus — showing the matrix, legend, lineage, and shape notes this skill produces
tags:
  - stack
  - concern/architecture
---

# Worked example: `notification-service/plateau/plateau-repository.md`

This is the file `plateau-map-create` produces for a fictional catalog at
`skills/example/architecture/notification-service/`. Its Variability Map
(`notification-service/variability-map.md`) has 4 VPs, every column filled:

| VP | Variants | Constraint | Realized by |
|----|----------|------------|-------------|
| VP1 Channel | `email` \| `sms` \| `push` (alternative) | — | `solution-email-channel`, `solution-sms-channel`, `solution-push-channel` |
| VP2 Templating | on \| off (boolean) | requires VP1 | `solution-mustache-templates` |
| VP3 DeliveryReceipts | on \| off (boolean) | requires VP1 | `solution-receipt-tracking` |
| VP4 RetryPolicy | `none` \| `fixed` \| `exponential` (alternative) | — | `solution-fixed-retry`, `solution-exponential-retry` |

Three plateaus exist on disk, built by `plateau-create-by-solutions`:
`plateau-core` (baseline, `standalone: false`), `plateau-transactional`
(`parent: plateau-core`), `plateau-marketing` (`parent: plateau-transactional`).

---

## Plateau × VP matrix

Rows = plateaus, columns = the 4 Variation Points. ✅ = the VP is realized at that
plateau, ❌ = it is not. Answers are **cumulative** down the chain — a plateau has
every VP its parent has, plus its own. Scan a **column** for the shallowest plateau
that includes a VP; read a **row** for a plateau's complete VP set.

| Plateau | VP1 | VP2 | VP3 | VP4 |
|---|:-:|:-:|:-:|:-:|
| plateau-core          | ❌ | ❌ | ❌ | ❌ |
| plateau-transactional | ✅ | ❌ | ✅ | ✅ |
| plateau-marketing     | ✅ | ✅ | ✅ | ✅ |

Column legend — VP1 Channel · VP2 Templating · VP3 DeliveryReceipts · VP4 RetryPolicy.
Full descriptions, the solution that realizes each VP, and the constraints between VPs
are in the catalog's `../variability-map.md` — the single source of truth; this table
is only the plateau-oriented view of the same answers.

- **VP1 is decided per message type** — a ✅ means the plateau *composes* at least one
  channel solution and its example demonstrates it, not that every message type uses
  every channel.
- **VP4 `none` is not a ❌** — `plateau-core` has no retry policy at all (❌);
  `plateau-transactional` composes `solution-fixed-retry`, so VP4 is ✅ there even
  though a caller may still select the `none` variant per message type.

## Lineage & new solutions

| # | Plateau | `standalone` | Parent | New solutions in its `created_by` (on top of the parent chain) |
|---|---------|--------------|--------|---------------------------------------------------------------|
| 1 | **plateau-core** | `false` | — | `solution-message-envelope`, `solution-dispatch-pipeline`, `solution-logging` |
| 2 | **plateau-transactional** | `true` | plateau-core | VP1 `solution-email-channel` + `solution-sms-channel` · VP3 `solution-receipt-tracking` · VP4 `solution-fixed-retry` |
| 3 | **plateau-marketing** | `true` | plateau-transactional | VP1 `solution-push-channel` · VP2 `solution-mustache-templates` · VP4 `solution-exponential-retry` (replaces `solution-fixed-retry` via DI re-registration — `TD-`, recorded in `registry/retrypolicy-cs`) |

## Constraint check

- VP2 `requires VP1`: `plateau-marketing` sets VP2 ✅ and VP1 ✅ — consistent.
- VP3 `requires VP1`: `plateau-transactional` sets VP3 ✅ and VP1 ✅ — consistent.
- No plateau sets VP2 or VP3 without VP1. No violations.

## Combinations the family allows that no plateau covers yet

`VP1 ✅` + `VP2 ✅` + `VP3 ❌` — templated messages with no delivery receipts — is a
legal combination with no dedicated plateau, because `plateau-marketing` (the only
plateau with VP2) inherits VP3 from `plateau-transactional`. Future work if a caller
needs it.
