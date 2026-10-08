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

## Owner-decided (2026-10-08, the Python example)

- The example gets a `make init` that prepares it.
- The `.gitignore` lines of an example must reach the `.gitignore` of the repository the skill is applied to.
- `make test-kinds` printed `unit tests coverage` — not readable; at least a separator.

## Agent decisions — the Python example

- `make test-kinds` prints `{kind} - badges: {badge} …` (`{kind} - no badge` without one). The kind stays the first word of its line, so the workflows' `cut -d' ' -f1` and `check.sh` are unchanged.
- `.gitignore` was specified nowhere: the base now names `tmp/` and `tools/livingdoc/node_modules/` as a MUST with a check-list line, each stack skill names its own lines, and `check.sh` §7 checks the two base lines in every example. The repository's own `.gitignore` already ignores `tmp/` and `.venv/`.
- `make init` creates `.venv` and installs the dev dependencies; the `Makefile` puts `.venv/bin` first on `PATH`, so the kinds use it with nothing to activate. Described in the Python `Repository.extend` as the way to give the kinds an environment. `run-example.sh` calls `make init` where an example has it.

## Owner-decided (2026-10-08, report and layout)

- The report's landing page lists its reports as "badge - link".
- A feature file lies beside the code it specifies and the tests beside it (`{folder}/features/`, `{folder}/test/`) — the Go skill is expected to say so.

## Agent decisions — report and layout

- The list is built by `test-report.sh` into the project's own page: the line `<!-- test-reports -->` of `report-template/index.html` is replaced by one item per `reports/{name}/` of the run, reports with a badge first. Built, not fetched by script in the page: a page opened from disk cannot fetch, and the list must be there. A page without the marker is published unchanged. Side effect: a `check` run no longer links reports it did not produce.
- The badge on the page is drawn from `badges/{name}.json` with three CSS classes in the template — a shields.io image needs the report to be published first.
- `cucumber-testing-in-go` got the layout as an explicit MUST: `solution-conformance-testing-in-go` already pointed at "its own convention", which the skill only implied through `Paths: "../features"`.
- ⚠️ Python is laid out differently — `features/` and `test/` at the repository root, from `solution-test-layout-in-python`'s ADR and the former `behave` convention — and was not changed: see `STATUS.md`.

## Owner-decided (2026-10-08, Python layout)

- Python moves to features and tests beside the code. `solution-test-layout-in-python` is removed; `cucumber-testing-in-python` defines where a feature and its tests live. The example is updated, and so is everything that referred to the removed skill.

## Agent decisions — Python layout

- Layout: `{package}/features/{rule}.feature`, `{package}/test/{rule}_steps_test.py`, a plain test `{package}/test/{module}_test.py`, no `__init__.py` in `test/`. The step module ends in `_test.py` like Go's `_steps_test.go`, so one pattern (`*_test.py`) collects everything and `*_steps.py` left `python_files`.
- `cucumber-testing-in-python` became a folder skill to hold the ADR (`adr/test-location.md`, with the costs measured on the example), and covers `pytest-bdd` only: the `behave` half described a runner the tool-choice ADR rejected, and `behave` needs its own `steps/` folder, which the layout rule cannot hold.
- The `pyproject.toml` lines the layout needs — packaging exclusion, `testpaths`, `--import-mode=importlib`, coverage `omit`, `mutmut` `do_not_mutate` — are in `solution-conformance-testing-in-python`; the rule skill links there.
- `plateau-python-cli` no longer composes a test-layout solution: its three test module skills are deleted and a `# Testing` section names the testing skills — INVARIANTS §1, "a base plateau names the testing skills it uses". Plateau ADR `remove-solution-test-layout-in-python`. This is W4 done for Python.
- `solution-cli-packaging` carried the same exclusion `"*test*"`, which drops any production package with `test` in its name (measured: `latest`); now `["*.test", "*.test.*"]`.
- `devops-github-action-check-changes-in-python`: tests sit below `src/`, so `code` needs negated patterns, which `dorny/paths-filter` evaluates only with `predicate-quantifier: every`. The patterns were checked against sample paths with `picomatch`, the library the action uses; the action itself was not run.

## Owner-decided (2026-10-08, who makes a badge)

- A kind's script decides how its result is produced and how the badge and the report are made from it; the badge is stored with the kind (`kinds/{kind}/badges/`). `test-report.sh` only gathers the finished pieces into the report.

## Agent decisions — who makes a badge

- The badge is still drawn in one place: three functions in `kind.sh` (`kind_badge_count`, `kind_badge_percent`, `kind_badge`) fix the schema and the colors. A kind chooses the name, the label and the numbers; it does not print JSON.
- The scenario page moved the same way: `kind_scenarios_report` in `kind.sh`, called by the unit kind, writing `report/scenarios/`. `test-report.sh` no longer reads any `result/` file.
- `result/*.json` stays, as the kind's own data with the same four shapes in every stack — the Go normalizers and anyone comparing runs use them — but it is no longer part of what the report builder needs.
- This removes the limit noted for the Angular kinds on 2026-10-08: a new kind publishes its badge and report by writing them, with no change to the shared builder.
- `test-report` now also fails on two kinds writing a badge of the same name; the three checks of `testing.sh` (a badge has a report, a declaring kind, and every declared badge of a kind that ran exists) are unchanged.

## Owner-decided (2026-10-08, living doc and the Go example)

- The Python living doc must show the type tags. The scenario page stays beside the living doc.
- `solution-conformance-testing-in-go` gets a runnable example like Python's.

## Agent decisions — living doc and the Go example

- Python tags: `unit_scenarios.py` completes the file `--cucumberjson` wrote — each `Examples:` row gets its own line and every tag it inherits, written `@tag` as godog does. It runs `trylast` in `pytest_sessionfinish`, after `pytest-bdd` has written the file. Checked on `pytest-bdd` 9.0 and 8.1.
- The Go example is the Python example's twin: `internal/linkcheck/` with the same feature file, its `test/` runner, and a `Makefile` with `init` (module download, `gremlins`) and the include line. `check.sh` §10 covers it.
- ⚠️ The example showed that the Go mutation kind measured almost nothing: `gremlins` runs the tests of the mutated package only, and under the layout rule that package has none — the scenarios run from `{package}/test/`. The limit was known (the common map's `agent/DECISIONS.md`; `gw009-001` said its mutation run "is not evidence"). `gremlins unleash --integration` runs the whole module's tests per mutant: the example went from 42.9% to 100%, and no example has a surviving mutant left. It is in `mutation.sh` now, with two settings it needs: `GOFLAGS=-count=1` (a cached first run gives every mutant a timeout of nearly zero — `gw009-001`: 81 timed out) and `--timeout-coefficient 10` (the default times out killable mutants of a suite that runs in under a second). The price is the whole suite per mutant.
- The Go mutation scores in `STATUS.md` changed with it; the remaining gap to 100% in the plateau examples is `noCoverage` — `main`, config and HTTP server code no scenario reaches.

## Owner-decided (2026-10-08, the report page and the examples)

- The separate "living documentation" line on the landing page is redundant: the `tests` item leads straight to the living doc.
- The examples get more features.
- A table "feature | scenario | scenario tags | …" should be available.

## Agent decisions — the report page and the examples

- `tests` → living doc: `kind_livingdoc` writes `report/tests/index.html` forwarding to `livingdoc/` when the runner's own report has no entry page (Go, .NET, Python). The landing page and a README badge link `reports/tests/` as before, so nothing in the builder knows about the living doc. TypeScript keeps cucumber-js's own HTML report as that page — the same renderer.
- The table is the scenarios page: `scenarios.json` gained `tags` (own and inherited, with `@`), and the page is one table — feature, scenario, examples, type, tags, status, location, note — under the type × status summary, instead of one table per feature.
- The three skill examples carry the same three features: a second feature in the same package, and a sub-package with its own `features/` and `test/` — data tables, an outline with two `Examples:` blocks, two `@todo` scenarios. Python also shows steps' shared fixture in `test/conftest.py`.
- Each feature carries one tag of the project's own (`@validation`, `@extraction`, `@batch`) to show what a feature-level tag looks like in the living doc and in the scenarios table. The skills prescribe only the type tags and `@todo`; a project's own tags are free, and in Python each must be a registered marker.

## Owner-decided (2026-10-08, feature tags)

- Features are tagged too: the tag says what the feature tests — infrastructure, service, mapping, and whatever other categories the plateaus call for (left to the agent).

## Agent decisions — feature tags

- Seven category tags, one per feature, on the `Feature:` line — the rule "One category tag per feature" in `cucumber-testing`. The set comes from the layers the plateaus actually have:

  | Tag | Go plateaus | .NET plateaus | Python CLI plateau |
  | --- | --- | --- | --- |
  | `@domain` | pure rule code | `{Module}.Domain`, `{Module}.Domain.Rules` | `functions/` |
  | `@service` | `internal/domain/services` | `{Module}.Application` | `command/` |
  | `@api` | `internal/api/http`, `grpc`, `tasks` | host endpoints | `cli/` |
  | `@infrastructure` | `internal/infrastructure/*`, TaskBox store | `App.Infrastructure` | `service/` talking to the outside |
  | `@mapping` | converters beside an adapter | mappers, persistence configuration | — |
  | `@contract` | `internal/domain/interfaces` | `{Module}.Interfaces`, `Shared` | — |
  | `@crosscutting` | `internal/config`, `internal/logging` | `BuildingBlocks` | logging set-up |

- The category follows the code the feature sits beside, so it needs no judgement per scenario; a feature that would need two specifies two things and is split.
- Made checkable like the type tag: `scenarios.json` carries `category` (`uncategorized` without exactly one), and the scenarios page shows the column, a category × status table, and marks `uncategorized` for attention.
- Every example feature is tagged (30 files). The three skill examples' illustrative `@validation` / `@extraction` / `@batch` tags were replaced by categories. One file is left untagged on purpose: `gw009-001`'s `taskbox-conformance.feature` is a verbatim copy from the `taskbox-go` library — its 23 entries show as `uncategorized` until the tag is added there.
- Component and pixel tests of a UI are other test kinds (owner, 2026-10-08), so there is no `@ui` category: a category classifies Cucumber features only.
- Scenarios inside the living doc — asked, not built. Measured: for classic Cucumber JSON (Go, Python) a `@todo` scenario added to the runner's report as a `pending` element renders in the living doc with its note; in Cucumber Messages (TypeScript, .NET) the scenario is already in the stream. The summaries and the `untyped` / `uncategorized` / `missing` marks have no place in either renderer.

## Owner-decided (2026-10-08, tag check)

- `@crosscutting` is unclear; the category is named `@tech-check`.
- `untyped` and `uncategorized` must be an error of the script, so an agent that only runs the tests sees that it did not tag everything.

## Agent decisions — tag check

- The check is `kind_scenarios_check` in `kind.sh`; every unit kind calls it after its results are written and exits non-zero when it fails — in a `check` and in a `report` run alike: it is a failed check, not a score. It prints one line per feature without a category tag and one per scenario without a type tag, with `file:line`.
- `missing` (a scenario no runner executed) and a `@todo` without a reason are not made errors: nobody asked, and `gw009-001` has one `missing` entry by design (`@store-transient`, no such store yet).
- `gw009-001`'s copy of `taskbox-conformance.feature` got `@infrastructure`, against "copied verbatim": without it the example's unit kind fails. Recorded in `STATUS.md` for the library's source.
- `@tech-check` has a hyphen; checked that it works as a registered `pytest` marker.
