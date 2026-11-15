# Architecture backlog cards — authoring contract

## Scope and invariants

The owner selected a directory of task cards, linked from the existing README
Backlog table. This implements the previously proposed fields: task status,
source links and stack scope, expected result, checks, dependencies and owner
questions. This contract records that agreed shape; it introduces no decision
about the architecture tasks themselves.

- Preserve all ten existing Backlog tasks, one card per task; keep the roadmap
  in README Status and the separate Library candidates inventory where they are.
- Make README Backlog the sole task index. Put shared execution guidance in
  backlog/README.md and link every card to it.
- Every card names its context, scope, dependencies, result, validation and open
  decisions. Record known facts separately from implementation proposals.
- Use statuses Ready, Needs decision or Deferred. Ready means requirements are
  sufficient to start; it does not authorize merging. Deferred cards state an
  observable trigger. Needs decision cards permit investigation, with owner
  agreement required before implementing the unresolved choice.
- Preserve owner decisions and update stale entry paths against this worktree.
  Do not implement queued tasks or choose unresolved architecture during this move.
- Local links resolve; every indexed card exists and every task card is indexed
  once. A mechanical check validates coverage, required sections and local links.

## Waves and status

| Wave | Classification / artifacts | State |
|---|---|---|
| A | New shared guidance and ten task cards; modify README index; new mechanical gate | done; checks pass |
| B | Fresh-eyes audit, final checks, PR for user review and merge | audit completed; corrected missing existing-client references; PR preparation |

## Decisions

- Owner: one directory, a card per task, all task-taking context inside the card,
  and links from the README Backlog table.
- Authoring: shared workflow is linked rather than repeated in ten cards.
- Authoring: a task requiring an owner choice is not presented as Ready; no new
  architecture choice is made while writing its card.
- Scope: roadmap VPs remain in the existing Status section; this move covers the
  ten Backlog rows, without silently expanding to the entire architecture roadmap.

## Validation evidence

- Ten original tasks have one indexed card each; no task implementation changed.
- The new gate passes and rejects broken-link, missing-section, status-mismatch
  and unindexed-card mutation probes; every probe was reverted.
- test/test.sh passes all structural gates and syncs 809 skills.
- Fresh-eyes review checked original task intent and source references; the
  existing correct Go HTTP/gRPC client conversions were added to the invalid
  loaded-entity card following that review.
