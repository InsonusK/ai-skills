# Taking an architecture backlog task

The [catalog Backlog table](../README.md#backlog) is the task index. Each linked
card contains the task's context, source locations, scope, start condition,
expected result, validation and open decisions. The roadmap VPs remain in the
catalog Status table; they are not silently included in this ten-task migration.

## Choosing a task

- **Ready:** requirements are sufficient to begin investigation and implementation.
- **Needs decision:** investigate and prepare concrete alternatives; agree the
  named architecture choice with the owner before implementing that choice.
- **Deferred:** the card names the event that unlocks it. Confirm that event has
  occurred before implementing; an inventory may be prepared earlier.

Check the current source and card before starting: links identify entry points,
not an exhaustive list of copies. Search tracked sources for dependents, inspect
conformance.copy and attach.allow where applicable, and include templates and
inherited examples when their owning contract changes. Do not edit generated
.agents/.claude copies. Follow the repository's current AGENTS.md and applicable
skills; work in your own worktree. Write new ADRs in the skill that owns the
decision rather than making this backlog a second architecture specification.

## Finishing a task

Use the card's expected result as acceptance criteria and its validation section
for task-specific evidence. Run checks from the repository root:

```sh
python3 test/check-architecture-backlog.py
bash test/test.sh
```

The latter requires ai-skill-manager and Python 3.11+ and runs catalog structural
checks and sync. When testing guidance changes, also run:

```sh
bash library/testing/agent/check.sh
```

For changed runnable examples, use each example's own README and Makefile:
Go `make init test lint`, Python `make init test lint`, .NET `make init test`.
Run in each affected example directory; .NET has no generic lint target here.
Database examples require a real PostgreSQL instance on which tests may create
and drop databases. Set TEST_DATABASE_DSN in that stack's accepted form (Go
postgres URL, Python SQLAlchemy PostgreSQL URL, .NET connection string), following
the example README. Use an isolated test instance, never a production database.
State any checks that were not run and why; a link check alone does not prove a
runtime change works.

Update the card with decisions, evidence and remaining work in the implementation
PR. Remove completed work from the active index or mark it completed consistently
with the card; preserve a PR/ADR reference for the outcome. If the directory later
contains completed cards, extend the checker/status vocabulary in the same PR.
Create a PR and ask the user to review and merge it. Do not merge it or enable
auto-merge yourself. A Ready status is a requirements assessment, not a delegation
to publish architecture decisions or merge branches.

## Maintaining cards

Keep task requirements in the card and shared execution instructions here.
README lists task, status and start condition only. Update both the index and
card status together. Keep local links valid and include a new card in the index
exactly once. The migration's scope and decisions are recorded in
[the authoring plan](../agent/backlog-cards/PLAN.md).
