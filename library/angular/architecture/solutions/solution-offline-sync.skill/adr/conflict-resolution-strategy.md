---
name: conflict-resolution-strategy
description: How conflicts are detected and surfaced when a queued mutation replays against an entity that changed on the server while the client was offline
problem: A queued mutation may target an entity whose server-side state has changed since the mutation was queued; we need a default resolution behavior, a way to inform the user specifically what differed, and a designed extension seam for smarter per-operation conflict logic later, without over-building that logic now
decision: Server wins by default; the queued command's own touched fields are compared against the current server values (returned by the backend on conflict) to show the user exactly what didn't apply — no full entity snapshot is stored client-side. Per-operation custom conflict resolution is an explicit, designed extension point for a future solution, not built now.
tags:
  - solution/offline-sync
  - concern/documentation
  - concern/documentation/adr
---

# Problem

While the client was offline, another actor (a different user, a background process) may have changed the same entity a queued mutation targets. On replay, this is a conflict. We need to decide: what happens to the local mutation by default (does it win, does the server's state win, does it require manual resolution); how much detail the user is shown about what specifically conflicted; and how much of a "smart," per-operation conflict-resolution system to build now versus deliberately deferring.

# Selected variant

**Selected variant:** [[#Server wins, field-scoped diff, extension point deferred]]

By default, the server's current state wins — a queued mutation whose target has changed is not force-applied. To tell the user specifically what happened, the backend is required to return, in its conflict response, the current values of only the fields the queued command itself intended to change (not the entire entity) — comparable directly against what the command intended, with no snapshot ever stored on the client. The architecture explicitly reserves a seam (a single, well-defined conflict-handling step in the replay orchestrator) for a future solution to plug in per-operation custom resolution logic (e.g. "a date change and a priority change from different actors are not considered conflicting"), but this solution does not build that logic — only server-wins.

# Searched variants

## Server wins, field-scoped diff, extension point deferred

### Description

On a 409-style conflict response, the backend returns the current values of the fields the failed command's payload touched (e.g. a command that set `priority` gets back the current `priority`, not the whole entity). The replay orchestrator compares the command's intended value(s) against these returned current value(s), discards the local change (server wins), and surfaces a notification — via the `notifications` global slice already sketched in the "State management" solution — describing the specific fields that differed and their current server values. The replay orchestrator's conflict-handling step is implemented as a single, clearly separated function/injection point, so a future solution can register alternative, per-operation resolution strategies (e.g. an "independent fields don't conflict" policy) without reworking the rest of the queue.

### Benefits

- No client-side snapshot storage required — the command itself already carries "what I intended to change," so nothing extra needs to be captured at enqueue time
- Requires the same minimal backend cooperation any conflict-detection approach would need (the backend must say *something* on conflict) — asking for only the touched fields, not the whole entity, is a smaller, easier contract to implement on the backend than requiring a full current-entity payload
- Gives the user meaningfully more information than a bare "conflict occurred" message, without the cost of building a full diff/merge UI
- The single conflict-handling seam is a deliberate design decision that keeps this solution's scope bounded (server-wins only) while making the future "smarter" resolution work additive rather than a rewrite

### Costs

- Server-wins by default means a user's queued change is silently (from the system's perspective) discarded when it conflicts — the user finds out only after the fact, via the notification, not before
- Requires backend coordination: every mutation endpoint needs to be able to return current values for the fields it accepts on a conflict response — a contract that has to be agreed upon and implemented, not something the frontend can deliver unilaterally
- Does not yet solve genuinely non-conflicting concurrent changes (e.g. the date/priority example) — those will still be treated as conflicts and discarded under this solution, until the future extension solution adds per-operation nuance

## Full entity snapshot at enqueue time (baseline diff)

### Description

When a mutation is queued, the client also stores a snapshot of the entity's full known state at that moment. On conflict, the backend returns its current full entity state, and the client diffs the stored baseline against the current state field-by-field.

### Benefits

- Can show a complete picture of everything that changed on the server, not just the fields the local mutation touched
- Diffing happens client-side, decoupled from exactly what the backend chooses to return (beyond the full current entity)

### Costs

- Requires capturing and storing a potentially large snapshot per queued mutation, growing the queue's storage footprint for no benefit beyond what field-scoped comparison already provides for this solution's stated goal (telling the user what their own change conflicted with)
- The stored baseline can itself be stale or incomplete if it was captured from partially-loaded client data, undermining the accuracy of the eventual diff
- Still requires the same backend cooperation (returning current state on conflict) as the selected variant, just for the whole entity instead of only the touched fields — no reduction in backend coordination cost for a real increase in client-side complexity

## Client wins (force local change through)

### Description

A conflicting mutation is force-applied regardless of what changed on the server in the meantime, overwriting the server's intervening change.

### Benefits

- The user's own action is never silently discarded — what they did offline always takes effect
- Simplest possible resolution logic — no comparison or user notification needed at all

### Costs

- Silently destroys another actor's legitimate change with no detection or notification, which is a worse default than server-wins for any genuinely concurrent multi-user system
- Provides no path to informing the user that a conflict even happened, since the local mutation always "succeeds" by construction

## Always require manual user resolution before replaying

### Description

Every conflict pauses the queue and requires the user to explicitly choose how to resolve it before that entry (or the whole partition) continues replaying.

### Benefits

- Maximum user control — no change is ever silently discarded or force-applied without the user's explicit decision
- Naturally extensible to arbitrarily complex per-field merge UIs later

### Costs

- Meaningfully more UI/UX work than a notification (a full conflict-resolution interface, not just a message), for a default policy this solution was not asked to provide — the user's own request was specifically "server wins, but tell me what happened," not "always ask me"
- Blocking queue replay on user interaction contradicts the goal of transparent, automatic background sync — most conflicts in practice may not need a human decision every time, especially before any smarter per-operation logic (the future extension) exists to distinguish genuinely conflicting changes from harmless ones
