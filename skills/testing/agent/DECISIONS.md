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

- Names `test-kinds`, `test-readme-check`, `TEST_RUN_PURPOSE` (`check` / `report`), `TEST_WORK_DIR`, `TEST_REPORT_DIR` — proposed, not confirmed.
- Purpose values name the purpose, not the trigger: a manual run is neither a PR nor a release.
- The README check compares against declared badges, not generated ones — a `check` run skips kinds, so generated badges would report false gaps; `test-report` cross-checks declared against produced in a `report` run so the declaration cannot drift.
- A self-skipping kind leaves a marker with the reason: absence alone cannot be told apart from a kind that wrongly decided not to run.
- A report may exist without a badge (today's scenario report has none); a badge always has a report.
- ~~Isolation is read as "no link into an architecture catalog"; design-method skills stay linkable.~~ Superseded by the owner: links only inside `skills/testing/`.
- Decisions are recorded here, not as ADRs yet: an ADR belongs to a skill whose body follows it, and the skills still state the current contract. Each ADR is written in the wave that changes its skill.

## Owner-decided (2026-10-07, after reviewing INVARIANTS)

- Isolation is strict: links only inside `skills/testing/`; every conflict is decided separately, not worked around.
- A stack-specialized skill may link its stack-agnostic base; the base only says a stack skill must be found, with no link (`ai-skill-manager` loads every linked skill).

## Agent decisions — W1

- `test-setup-check` renamed `test-readme-check`, `gate` renamed `check`: the owner found both names unclear. Still proposals.
- The topic folder is named exactly after the skill, so the layout is checkable: `{skill-name}` or `{skill-name}-in-{stack}`, nothing else.
- Renamed to fit that rule: `no-test-theater-{angular,dotnet,python}` → `no-test-theater-in-…`; `dotnet-unittest` → `unittest-in-dotnet`; `testing-strategy` → `testing-strategy-in-dotnet`. The last two have no base — accepted by the owner for one-stack topics; `skill-design` now states it.
- `-in-angular` is kept although Angular is a framework on `stack/typescript`; `check.sh` requires `framework/angular` for it.
- Relative links between testing skills were made repository-absolute before the move, the form the rest of the repository uses.
- Four links out of `skills/testing/` became plain names without asking: three only named an example or a consumer, one (TypeScript tool-choice ADR) pointed at the dotnet catalog solution for a fact that now lives in `solution-conformance-testing-in-dotnet`.
- The location rule went into `skill-design` as a MUST with its own ADR: the old rule placed a base under `skills/common-workflow/` and a one-stack skill under a plain name, both of which this move contradicts.
- `.validation` logs were not edited by hand (their header forbids it).
- ⚠️ Go `Makefile` dependency and Python `solution-test` dependency — see `STATUS.md`.

## Owner-decided (2026-10-07, layout)

- `skills/testing/core/` + `skills/testing/{stack}/`, not folders by topic: `ai-skill-manager` selects by path, and a topic folder mixes every stack.

## Agent decisions — regrouping

- A skill sits directly in `core/` or `{stack}/` with no per-topic wrapper folder — the wrapper would hold exactly one skill.
- `-in-{stack}` stays on every stack skill, including the two with no base: the generated copies (`.claude/skills/`, `.agents/skills/`) are flat, so the name is what states the stack there.
- `angular/` is its own folder although its stack tag is `stack/typescript`: an Angular project lists `core`, `typescript`, `angular`.
- `check.sh` also forbids a stack skill linking another stack's skill (Angular → TypeScript allowed) — the same loading argument as base → extension.
- The by-topic ADR written in W1 was replaced, not kept as history: it was never merged.

## Owner-decided (2026-10-07, isolation conflicts)

- Two skills may both say "create the `Makefile`"; what matters is that their content does not conflict, and one file results. Solutions and plateaus accept that they build on `skills/testing/` and shape their own logic around it.
- How Python tests are laid out (`test/` mirrors the sources, `{Module}_test.py`, excluded from the package) is a general Python testing requirement and belongs in `skills/testing/python/`.

## Agent decisions — W1a

- `solution-test` → `solution-test-layout-in-python`: the `solution-` prefix stays because it keeps its `Implementation/` files and plateau `created_by` links; `test-layout` says what it decides.
- The Go skill's `Repository.extend` file keeps its name; only its text now says the `Makefile` is created when missing. Renaming it to `.create` would make two `.create` files on one element in the Go catalog.
- `validation-config.yaml` excludes `testing/*/solution-*.skill/**`, not only the conformance solutions, so the moved layout solution keeps being treated as a solution skill.
- dotnet, python and typescript testing skills already add their own `Makefile` without assuming a catalog's — no change needed there.

## Owner-decided (2026-10-07, before W2)

- Add the stacks to the dev container; write everything that can be written without running it; leave a task for the next agent to run the examples after the container is rebuilt.

## Agent decisions — W1b, W2, W3

- The caller-facing half of the contract is two files copied verbatim into every project (`tools/testing/testing.mk`, `testing.sh`), like `tools/livingdoc/`: it could be tested here, and a stack only supplies recipes.
- Kinds live in `{work}/kinds/{kind}/`, not `{work}/{kind}/`, so the report directory can default to `{work}/report` without a kind ever colliding with it.
- Each kind's `result/` and `report/` keep the file names of the old `tmp/result` and `tmp/report`, to keep the change in the tools to their output root.
- `test-report` copies every `kinds/*/report/{name}/` and looks results up by file name across kinds, so a new kind's report is published without changing the report builder.
- `run.json` (purpose; per kind `ran` / `skipped` / `missing` and its note) is how the report shows the mode; the landing page displays it.
- `pr-check` renamed `check`: the publish workflows gate on the same fast run, and they are not pull requests.
- Under `check`: unit reports no coverage (dotnet/typescript do not collect it either — as before); mutation runs over code changed since `DELTA_BASE` and skips itself without one. The delta mode is what the scripts already supported for a manual run.
- The pull-request workflow passes no `DELTA_BASE`: the DevOps rule "never gate a PR on mutation testing" is deliberate and stays true. ⚠️ in `STATUS.md`.
- The report workflow puts `continue-on-error` on every kind step — it cannot name the mutation kind. ⚠️ in `STATUS.md`.
- Python keeps `behave`; `behave-cucumber-formatter` supplies classic Cucumber JSON. The existing tool-choice ADR already selected behave, so this closes its open `TODO` rather than reopening the choice.
- `scripts/test-report.sh` and `normalize-scenarios.sh` are one file for dotnet, python, typescript (the skills already required byte-identity).
- Example `README.md` files with badges were added: `test-readme-check` needs them; the URLs are placeholders under `example.github.io`.
- ADRs written before today keep the old target names — they record decisions made under the old contract.
- Angular catalogs do not use this `make` contract and were not touched.
