# Review extraction of fixed code into libraries

Status: Deferred

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

Library candidates are listed in the catalog README with their current owners
and reasons for extraction. The owner agreed to revisit the whole list once the
variation points are formed (2026-10-18). Identical code is a candidate, not an
approved package boundary or publishing plan.

## Sources and scope

- [Library candidates inventory and roadmap](../README.md)
- [Go fixed templates and consumers](../go)
- [.NET fixed templates and consumers](../dotnet)
- [Testing tooling contract](../../testing/core/solution-conformance-testing.skill)

Review the complete candidate list, including fixed Go/.NET infrastructure,
testing tooling and TaskBox. TaskBox has its own prior library decision; do not
infer the same readiness for other candidates. This card produces an extraction
plan, not immediate publication of every proposed package.

## Dependencies and start condition

Start the review when the owner confirms the VP set is mature enough. The old
"after step 4" note refers to TaskBox/Inbox/Outbox in Status; it does not mean
those planned VPs are already implemented.

## Expected result

1. Inventory byte-identical code versus project-specific substitution and
   identify real consumers for each candidate.
2. Discuss public API, dependency direction, repository/package ownership,
   versioning, compatibility and migration cost with the owner.
3. Record accepted/rejected/deferred candidates and their reasons. Create
   separately scoped implementation tasks for accepted extractions, each with
   consumer migration and release validation.

## Validation

For each proposed boundary, show how an existing example imports it and what
project-specific code remains. Require a buildable consumer proof before marking
an extraction ready. Shared checks must pass for any documentation changes;
no package is published as a side effect of this review.

## Open decisions

Owner approval is required for selection, package boundaries, hosting,
maintenance/versioning and publication. None of those choices is settled by
this card.
