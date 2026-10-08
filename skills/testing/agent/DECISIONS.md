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

## Owner-decided (2026-10-07, after W3)

- `make test-kind-unit` exits non-zero when a test failed, and CI must see it: the report workflow has no `continue-on-error`. A failed kind turns its job red and the report is still published.
- Python: `pytest-bdd` instead of `behave` + plugin — `behave` has no advantage here that pays for the extra formatter.
- Testing skills deliver code as real files (`skill-code-delivery`), not as markdown examples.

## Agent decisions — W6 and the exit-code rule

- New contract rule: a kind exits non-zero only for a failed check; never for a score in a `report` run. It is what lets the report workflow drop `continue-on-error` without going red over a surviving mutant.
- The `pytest-bdd` switch is recorded as `TASK.md` item 9, not written here: Python has no runnable example and no interpreter in this container, and the details that matter (what the Cucumber JSON reports for outline rows, marker mapping for `@todo`) can only be settled by running it.
- Files identical for dotnet, python and typescript (`normalize-scenarios.sh`, `test-report.sh`, `messages-results.jq`) live once, in the core skill's `assets/`; stack skills reference them.
- The descriptions of the stack scripts stay as `templates/*.md` beside the real files — moving them would rewrite links for no gain in delivery.
- Go's `Makefile` block is an asset appended to the project's `Makefile` (`assets/Makefile.testing`), although `skill-code-delivery` would call a merged fragment an instruction: it is identical in every project, and the examples are checked to contain it verbatim.
- dotnet placeholder `{Solution}` became `{solution}` (kebab-case, as the standard requires).

## Owner-decided (2026-10-07, structure of the contract)

- One Makefile, one report script for every stack, and per stack only the scripts that run the tests and produce JSON. Go must not differ from the other stacks.

## Agent decisions — W7

- A kind is a file, not a declaration: `tools/testing/kinds/{kind}.sh`. `make test-kinds` lists the folder; the badges come from the script's `# badges:` line. This removed `TEST_KINDS` / `TEST_BADGES_{kind}` and every recipe from project Makefiles.
- Build and install steps moved into the kind scripts (dotnet restore/build, `pip install`, `npm install`): a kind must run from a clean checkout without a Makefile prerequisite.
- The dotnet unit kind finds the solution file itself, so the script is an asset with no placeholder.
- Go keeps its three normalizers in Go (`go test -json` leaf counting, the gherkin inventory, the gremlins report) — they are how that stack "produces JSON"; its Go report builder is gone in favour of the shared `test-report.sh`, which makes `jq` a requirement for Go projects too.
- The Go mutation kind decides to skip before it touches the toolchain, so a `check` run costs nothing.
- Kinds run in alphabetical order under `make test-and-report` — they are independent, so the order carries no meaning.
- Per-file description documents (`templates/*.md`, the `testing.mk` / `testing.sh` / `test_report` Implementation files) were deleted: with real files, `Repository.create` / `Repository.extend` reference them in one line each.

## Agent decisions — W5 (running the examples)

- `testing.sh` writes each kind's exit code to `{kind}/exit-code`; `run.json` gained the state `failed`, and `test-report` exits `0` when a kind failed. Without it the owner's rule "a failed kind turns its job red and the report is still published" never held for a red test: both mutation tools need green tests, so the mutation kind stopped without a result, `test-report` exited `1` over its missing badge, and the workflow skipped the publish step. No target or variable was added; the workflows are unchanged.
- `test-report.sh` writes `reports/{name}/index.html` when the tool wrote none: the landing page and every README badge link `reports/{name}/`, which is a 404 on a static host for Go and .NET `reports/tests/`.
- `normalize-scenarios.sh` ignores everything below `TEST_WORK_DIR`: a mutation tool's sandbox there holds copies of the `.feature` files.
- Go: `--threshold-efficacy 0 --threshold-mcover 0` in a `report` run. `gremlins` exits `0` on survivors by default (TASK item 3), but `10` once `.gremlins.yaml` sets a threshold.
- Go: `normalize_unittest` names the failed tests on stderr — the run's only other output is the JSON stream.
- .NET: delta scoping through `--mutate` patterns built from `git diff`, not `--since`. Measured: with Reqnroll-generated tests Stryker.NET 4.16.0 logs a changed production file as "Changed test file" and ignores its mutants. This also makes the three non-Go stacks scope a delta run the same way.
- Every non-Go mutation kind resolves `DELTA_BASE` with `git rev-parse` and compares against the working tree, so `HEAD~1`, a branch or a tag all work, and the kind skips itself ("no … file changed") instead of running over nothing.
- Python: `pytest-bdd` implemented (TASK item 9). The scenario results come from a small `pytest` plugin, `tools/testing/kinds/unit_scenarios.py`, not from `--cucumberjson`: measured, `pytest-bdd` 9.0 reports the outline's line for every `Examples:` row and drops the `Examples:` tags, so its JSON cannot tell two blocks apart. The plugin uses the documented `pytest_bdd_before_scenario` hook and reads the row lines from the feature file; checked on `pytest-bdd` 8.1 and 9.0.
- Python: `mutmut` 3.8 — counts from `mutmut export-cicd-stats`, delta scoping through mutant-name patterns (it has no path option), `./mutants` deleted afterwards (it accepts no other directory). Both former `VERIFY` placeholders are gone, so `mutation.sh` became an asset with no placeholder.
- Python and TypeScript got an `example/` inside their `solution-conformance-testing-in-{stack}` skill — the minimal package each was proved on — and `check.sh` §10 covers them. Before, neither stack had anything runnable.
- TypeScript: only what the run broke was fixed in the kind scripts (git pathspec, Stryker sandbox and `c8` temp directory below the kind directory). The Vitest half of that skill was not touched — ⚠️ in `STATUS.md`.
- `run-example.sh` and `report-links.py` in this folder repeat W5 for one example; they need the toolchains, so `check.sh` does not call them.
- `no-test-theater-in-angular` was not re-stamped in `.validation`: `validation_queue.py` registered the moved skills as never validated, and a stamp means a validation that did not happen here.

## Owner-decided (2026-10-08, after W5)

- Front-end tests are several test kinds: pixel (visual) tests, component tests, and tests of services and classes. Only the last become Cucumber scenarios. Every kind runs through `make test-kind-{kind}`, so CI picks it up with no workflow change.
- The name of the Cucumber JSON file a Go runner writes does not matter, as long as the HTML report is built from it correctly. Nothing changes there.
- Go delta mutation in a module below the repository root: try to make it work.

## Agent decisions — after W5

- `solution-conformance-testing-in-typescript` drops Vitest: with services and classes tested by scenarios, the framework-agnostic package has one runner, `cucumber-js`, and `c8` for its coverage — which is what the unit kind already ran. `cucumber.mjs` and `stryker.conf.json` became assets. The ADR keeps Vitest as the rejected variant.
- The Angular kinds (component, pixel) are not written: the Angular catalogs are not on the `make` contract yet, so that is a wave of its own — a `solution-conformance-testing-in-angular` with one kind script and one badge name per kind. Listed in `STATUS.md`.
- Go delta mutation below the repository root works through git's `diff.relative`, set for the `gremlins` process only (`GIT_CONFIG_COUNT`/`KEY`/`VALUE`): measured in `plateau-http-service` — 27 mutants `SKIPPED` without it, the 2 on the changed line run with it. No effect when `go.mod` is in the root.

## Owner-decided (2026-10-08, obsolete skills)

- `workflow-unittest-testplan` (with its `usecases_list.md` template) and `unittest-in-dotnet` are removed: a hand-kept list of cases beside the `.feature` files is what the scenario report replaced.
- Plain (non-Cucumber) tests stay only for a case whose implementation through Cucumber is unjustifiably complex. Technical tests are scenarios too — a person reads Cucumber text more easily than test code.

## Agent decisions — obsolete skills

- `cucumber-testing`'s "One scenario, one runner" named the runner entry point as the only exception; it now names the second one, and asks for the reason in a comment above such a plain test — the comment is the agent's addition, so the exception stays visible in review.
- `test-driven-development` and `solid-decomposition` take their test cases from the unit's `.feature` file (`@todo` scenarios) instead of `usecases_list.md`. The step "show the list of cases to the user for confirmation" went with the removed skill; `solid-decomposition` still confirms the decomposition itself.
- Lost with `unittest-in-dotnet` and not moved anywhere: the xUnit class template (`LoggingTestsBase`, `test_{group}_WHEN_{condition}__THEN_{result}`), "mocks in a separate folder", and the `{Project}.Test` / `{Class}_Test.cs` layout — the last contradicts `INVARIANTS.md` §1 (`{Project}.Tests`) and is W4's subject.
- `moves.tsv` keeps both rows: it records W1.
