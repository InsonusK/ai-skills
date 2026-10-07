# Decisions log

One line per non-mechanical choice. ⚠️ = a genuine architectural fork, waiting on the owner.

## Owner-decided (2026-10-07, chat)

- All testing skills — stack-agnostic and stack-specialized — move to `skills/testing/`, breaking `skills/{stack}/{concern}` on purpose: a failing test is traced across the agnostic skill, the stack skill, and other stacks' skills, so they sit together.
- Grouping by topic: `{topic}/{topic}.skill` beside `{topic}-in-{stack}.skill`.
- `skills/testing/` is isolated: no link into an architecture catalog, enforced mechanically.
- Test layout is a convention derived from the program's structure (dotnet: a test project beside every production project), not something a plateau describes.
- Pluggable modules: every external integration lives in its own infrastructure project/package; a general "testing pluggable components" skill puts a test project/package beside it.
- Existing programs: the agent fixes small deviations; large changes only as a separate task or on the user's direct instruction.
- DevOps contract: `make test-kind-*` in parallel, then `make test-report`; DevOps never knows the kinds. CI discovers kinds through a list target.
- Run parameters state facts about the run, not how to test; a kind logs what it does because of them. Replaces DevOps setting `ONLY_DELTA` / `WITH_CODE_COVERAGE`.
- Output directories are chosen by the caller; `public/` may belong to the project.
- A badge and its report share a name; a kind may produce several.
- README badges are not written by CI; a check fails with an explicit "badge missing in README" message, and a person or agent adds the line.
- VP-specific test detail (dotnet domain logic) stays with the VP; the attachment mechanism is deferred.

## Agent decisions

- Names `test-kinds`, `test-setup-check`, `TEST_RUN_PURPOSE` (`gate` / `report`), `TEST_WORK_DIR`, `TEST_REPORT_DIR` — proposed, not confirmed.
- Purpose values name the purpose, not the trigger: a manual run is neither a PR nor a release.
- The README check compares against declared badges, not generated ones — a `gate` run skips kinds, so generated badges would report false gaps; `test-report` cross-checks declared against produced in a `report` run so the declaration cannot drift.
- One `test-setup-check` target rather than a README-specific one, so DevOps runs a static check without knowing what it covers.
- A self-skipping kind leaves a marker with the reason: absence alone cannot be told apart from a kind that wrongly decided not to run.
- A report may exist without a badge (today's scenario report has none); a badge always has a report.
- Isolation is read as "no link to a plateau, catalog solution, Variability Map, or Feature Model"; design-method skills stay linkable.
- Decisions are recorded here, not as ADRs yet: an ADR belongs to a skill whose body follows it, and the skills still state the current contract. Each ADR is written in the wave that changes its skill.
