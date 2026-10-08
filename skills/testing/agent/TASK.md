# Task: bring every stack to the Python reference (W16)

You continue the `skills/testing` work in the worktree `.ai-worktree/skills-testing`, branch `skills-testing` (local, never pushed, no pull request). Read `INVARIANTS.md`, `STATUS.md` and the last twenty entries of `DECISIONS.md` in this folder first; `AGENTS.md` at the repository root holds the project rules.

## Why this task exists

For two days the owner shaped the testing skills by looking at one runnable example and asking for changes: the layout, the tag scheme, the report page, the living doc. At the end the owner narrowed the work to the Python example alone, so the last decisions were finished there and nowhere else. That example is now what the owner approved. The other stacks are one or more decisions behind it, and several skills still describe what was removed.

Your job is to make the Go, TypeScript and .NET stacks match the Python reference — each with a runnable showcase example of its own — and to bring every skill that talks about testing into line with it.

The owner reviews by opening an example's report in a browser, not by reading diffs. An example that runs and shows the right page is the proof; a skill text that says so is not.

## The reference

`skills/testing/python/solution-conformance-testing-in-python.skill/example` — run it before anything else:

```bash
cd skills/testing/python/solution-conformance-testing-in-python.skill/example
make init && make test-and-report      # then read tmp/testing/report/index.html
```

Expected on 2026-10-08: exit 0, 26/26 tests, coverage 99.2%, mutation score 87.4%.

The skills behind it, also part of the reference: `skills/testing/python/solution-conformance-testing-in-python.skill` and `skills/testing/python/cucumber-testing-in-python.skill`.

What the reference shows, and every other stack has to show too:

1. **Layout.** A feature lies beside the code it describes, its tests beside it: `{package}/features/{rule}.feature`, `{package}/test/…`. The rule and its ADR belong to the stack's `cucumber-testing-in-{stack}` skill.
2. **One domain, eight features.** The `linkcheck` package: `check` and `extract` (`@type/domain`), `batch/summary` (`@type/service`), `cli` (`@type/api`), `store` (`@type/infrastructure`), `mapping` (`@type/mapping`), `contract` (`@type/contract`), `package` (`@type/tech-check`). Every feature has real production code behind it; no step re-implements a rule.
3. **Every tag at least once.** All seven `@type/…` values, all seven `@category/…` values (`happy`, `boundary`, `negative`, `error`, `concurrency`, `security`, `regression`), two `@status/todo` with a `# todo:` reason, one `@status/broken` with a `# broken:` reason, one `@status/validated`. A `Scenario Outline` with two `Examples:` blocks carrying different categories.
4. **The unit kind fails on a missing tag.** A feature without exactly one `@type/…`, or a scenario or `Examples:` block without exactly one `@category/…`, fails `make test-kind-unit` and names the offender.
5. **The living doc is the one place a reader sees the scenarios.** It lists every scenario with all its tags — the excluded ones too, each with its reason — and carries the status legend. The `tests` line of the report page opens it directly.
6. **No scenarios page.** The owner removed it on 2026-10-08: once the living doc lists every scenario with its tags, the page repeats it. `result/scenarios.json` stays — the tag check and the living doc read it.
7. **The report page** is a list of "badge - link" lines; a kind writes its own badges and reports, `test-report.sh` only gathers.
8. **`make init`** prepares the example when it needs preparing; the example has its own `.gitignore`; `make test-kinds` prints `{kind} - badges: …`.
9. **Both kinds measure something.** Tests, coverage and mutation score are real numbers from a real run, and delta mutation mutates only the changed files.

`@status/validated` in the reference was placed by the agent at the owner's request, to show the tag. The rule is unchanged: only a person sets it, and you remove it from any scenario whose text or steps you change, and say so in your report. In a new example put one, on a scenario you then leave alone, and list it in your report so the owner can confirm or remove it.

## What is already different from the reference

Verified on 2026-10-08, commit `5ac788ab` plus this commit.

| | Go | TypeScript | .NET |
| --- | --- | --- | --- |
| Runnable example in the testing skill | yes, three features | yes, three features | **none** — only the three plateau examples |
| Layout | beside the code | root `features/` and `features/step-definitions/` | separate `*.Tests` projects |
| Living doc built from | classic Cucumber JSON, `multiple-cucumber-html-reporter` | Cucumber Messages, `@cucumber/html-formatter` | Cucumber Messages, `@cucumber/html-formatter` |
| Excluded scenarios in the living doc | added by `render.mjs` | never looked at | never looked at |
| Status legend in the living doc | page footer | absent with one Messages file | index page only, with several files |
| Scenarios page | still written | still written | still written |

In the Python stack only `tools/testing/kinds/unit.sh` stopped calling `kind_scenarios_report`. Everything shared is untouched: `kind.sh` still holds the function, `test-report.sh` still has a hint line for it, and the core skill still has a `## Scenario report` section describing a page.

`run-example.sh` no longer requires `reports/scenarios/index.html`; it does not forbid it yet.

## The work

Order matters: the shared files first, because every example copies them.

### 1. Remove the scenarios page everywhere

- `skills/testing/core/solution-conformance-testing.skill/assets/tools/testing/`: delete `kind_scenarios_report` from `kind.sh` and whatever only it used; delete the `scenarios` hint in `test-report.sh`. Keep `kind_scenarios_check`, `normalize-scenarios.sh` and the legend the living doc needs.
- The `unit.sh` of Go, .NET and TypeScript: drop the call.
- Every example's copies, in the same commit as the asset they copy.
- `run-example.sh`: turn the removed requirement into `hasnt reports/scenarios`.
- Texts: the core skill's `## Scenario report` section becomes a description of `result/scenarios.json` and what reads it (its anchor is linked from other skills — fix the links); then every file `git grep -lE 'kind_scenarios_report|reports/scenarios|report/scenarios|scenario report|scenarios page|#scenario-report' -- skills` lists. `INVARIANTS.md` §5 names "the scenario page" among `kind.sh`'s duties.

### 2. The living doc shows the same thing in every stack

This is the hard part of the task, and the one most likely to be reported done when it is not.

Go and Python go through one renderer that this branch controls. TypeScript and .NET go through `@cucumber/html-formatter`, which renders a Messages stream as it is. Nobody has checked what that page shows for a scenario excluded by the tag filter, and with a single Messages file it has no legend at all.

Required in all four stacks: every scenario of every feature is on the page with all its tags; a `todo`, `broken` or `not-run` scenario is there with its reason; the legend of the statuses is on the page.

How is yours to choose. Options seen so far: add the missing scenarios and the legend around the Messages page; or convert Messages to classic JSON and use the one renderer, which also gives the four stacks one look. Weigh the second seriously — the owner compares the pages side by side. Whatever you choose stays in `tools/livingdoc/`, shared and identical in every example, and off the project's own `package.json`.

The container has no browser. Verify from the generated HTML and its embedded data: for each stack's showcase, a script that asserts all 17 tags and both reasons are present, and that no scenario of `result/scenarios.json` is absent from the page. Add that assertion to `run-example.sh` so it runs for every example from then on. `[object Object]` in a header was the last defect found this way — grep for it.

### 3. Showcase examples

Port the reference, feature for feature, so the owner can lay the four reports side by side. Same domain, same feature names, same scenarios, same tags; the code is idiomatic for the stack, not a transliteration.

- **Go** — `skills/testing/go/solution-conformance-testing-in-go.skill/example`: extend from three features to the eight.
- **TypeScript** — `skills/testing/typescript/solution-conformance-testing-in-typescript.skill/example`: extend to the eight and move to the co-located layout (see the decisions below). Today's rule that a step definition imports only from `src/index.ts` has to be reconciled with tests lying beside an internal module; the build must leave the tests out of the published package. Write the ADR the way `cucumber-testing-in-python.skill/adr/test-location.md` does, with the real costs.
- **.NET** — create `skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/example`: a small solution on Reqnroll, xUnit v2 and Stryker.NET (versions as in the skill and its xUnit ADR). Keep the separate `*.Tests` project layout the skill describes today.

Each example then gets a `# Ground truth` section in its skill with the numbers you measured, and a line in `check.sh` §10 so its kind scripts are compared with the stack's assets (Go, Python and TypeScript are there already).

A concurrency scenario and a file-store scenario exist in the reference; they must be deterministic in your stack too. A flaky showcase is worse than a missing one.

### 4. Stack skills and the rest of the repository

For each of Go, TypeScript, .NET — and the one Angular skill, `no-test-theater-in-angular` — read every file of `skills/testing/{stack}/` against the reference and the core skills, and fix what disagrees: layout, tags, filters, what the kinds write, the feature and step templates, the check lists. The Python skills show the wording that was accepted.

Then outside `skills/testing/`:

- Feature templates without tags — a project generated from them fails its first unit run. Known: `skills/dotnet/architecture/solutions/solution-cecil-architecture-tests.skill/Implementation/{Module}.Domain.Tests.csproj.extend/{Check}.feature.create.md` and `skills/dotnet/architecture/solutions/solution-domain-shared-rules.skill/Implementation/{Module}.Domain.Rules.Spec.create/{Rule}.feature.create.md`. Search for more: fenced `Feature:` blocks in any `skills/**/*.md`.
- `solution-domain-shared-rules` mentions the scenarios page in two files.
- The nine plateau examples (six Go, `gw009-001`, three .NET): shared files and kind scripts updated with the assets, then re-run. Do not turn them into showcases.

A changed skill gets its `version:` raised. The core solution skill uses an integer (now `8`), the others a timestamp.

### 5. Close

- `STATUS.md`: a W16 row, the table of measured results for all thirteen examples, and the "Waiting on the owner" list pruned of what you closed.
- `DECISIONS.md`: every choice you made that the owner did not.
- `python3` validation queue as before: `validation_queue.py queue --dry-run`.

## Decisions already taken — do not reopen

- Tag scheme, the closed lists of values, exclusion of `todo` and `broken` from the run, `none` and `not-run` as report words (`DECISIONS.md`, W13–W15).
- Every test of a package is a Cucumber scenario; a plain test only where a scenario would be unjustifiably complex, with the reason in a comment.
- The TypeScript skill has no Vitest. Front-end component and pixel tests are test kinds of the UI framework's own testing solution.
- The caller contract of `INVARIANTS.md` §5: no new target, no new caller-facing variable.
- The name of the Cucumber JSON or Messages file does not matter to the owner, only that the page is built from it correctly.

## Decided by default — the owner may overrule before you start

Written by the previous agent, not by the owner. If the owner changed one, it is noted here.

- **TypeScript moves to the co-located layout.** The owner asked for it in Go and Python and was never asked about TypeScript.
- **.NET keeps separate test projects.** Tests inside a production assembly are not idiomatic there, and the catalog's layout question is W4.
- **The plateau examples stay as small as they are.**
- **Go: the scenario inventory stays per outline, not per `Examples:` block.** With the scenarios page gone nothing shows a per-block status any more. Fix it only if the living-doc work needs it.

## Not in this task

- **Angular test kinds** (component and pixel tests behind `make test-kind-*`). No browser in the container, and the design is not decided.
- **W4** — moving the test layout and the plateau `*.Tests` structure skills of the architecture catalogs into `skills/testing/`. It needs owner decisions listed in `STATUS.md`.
- Workflows on GitHub, consumers' `ai-skills.yaml`, the `taskbox-go` source repository, a pull request.

If one of these blocks you, write it in `STATUS.md` under "Waiting on the owner" and go on with the rest.

## Rules of the branch

- Edit under `skills/` only; `.claude/skills/` and `.agents/skill/` are generated.
- Code has one source: a real file under a skill's `assets/` (copied verbatim) or `templates/` (placeholders filled). Change it there **and** in every example copy in the same commit — `check.sh` §6 and §10 fail otherwise.
- Shared by every stack, in `skills/testing/core/solution-conformance-testing.skill/assets/`: `tools/testing/` and `tools/livingdoc/`. Stack-specific: only `tools/testing/kinds/`, plus for Go the three normalizers and for TypeScript `cucumber.mjs` and `stryker.conf.json`.
- A project's `Makefile` carries `include tools/testing/testing.mk` and no testing recipe (`check.sh` §11).
- One commit per wave, on `skills-testing` only, `check.sh` green and the affected examples re-run before each. Nothing is pushed.
- If the caller contract itself looks wrong, record it in `DECISIONS.md` with ⚠️ and stop for the owner. Anything else that looks wrong — say so, with the reasoning, and carry on with your recommendation.

## How to verify

1. `bash skills/testing/agent/check.sh` — copies and examples consistent.
2. `bash skills/testing/agent/run-example.sh {example-dir}` for every example: a `report` run, a `check` run with caller-chosen directories, delta mutation, a link check of the report. It needs the example's toolchain, `jq`, `node` and `python3`; it runs the example's `make init` itself.
3. Per stack, once: break one scenario → `make test-kind-unit` exits non-zero, `result/scenarios.json` exists, `make test-report` exits 0 and `run.json` shows the kind as `failed`. Remove one `@category/…` tag → the unit kind fails and names the scenario.
4. Delta mutation on a real change needs a repository whose `HEAD~1` differs in a production file: copy the example out, `git init`, commit twice, `make test-kind-mutation TEST_RUN_PURPOSE=check DELTA_BASE=HEAD~1`.

Undo a deliberate breakage from a copy of the file, never with `git checkout` while other changes are uncommitted: that is how two retagged feature files were lost and committed wrong in `d16859c6`.

## The environment

Go 1.26, .NET SDK 10, Node 24, Python 3.13; no Docker, no browser, no PostgreSQL.

`gw009-001` needs a PostgreSQL the run may write to, through `TEST_DATABASE_DSN`. It ran against binaries unpacked without root: `https://repo1.maven.org/maven2/io/zonky/test/postgres/embedded-postgres-binaries-linux-amd64/{version}/embedded-postgres-binaries-linux-amd64-{version}.jar` → unzip → `tar -xJf postgres-linux-x86_64.txz` → `bin/initdb`, `bin/pg_ctl -o "-p 55432 -k {dir} -c listen_addresses=127.0.0.1" start`; the archive has no `createdb`, so the DSN names the default `postgres` database. That example takes about eleven minutes.

Known traps, each cost an hour once:

- Go mutation: `gremlins` needs `--integration`, `GOFLAGS=-count=1` and `--timeout-coefficient 10`, or it measures nothing or reports false timeouts. The script has them; keep them.
- Stryker.NET: delta goes through `--mutate` patterns from `git diff --relative`, not `--since`.
- StrykerJS copies the project into a sandbox; a normalizer that walks the tree counts every scenario twice unless the sandbox is under the work directory, which it ignores.
- `multiple-cucumber-html-reporter` 4.4.2 takes `customData` as a flat key-to-string object; anything else prints `[object Object]`.
- In a script, `ls a b | head -1` under `pipefail` dies silently when one pattern matches nothing.

## What to hand back

A message to the owner, in Russian, that starts with what the owner can now open and see: the four showcase reports and where they are. Then a table of the thirteen examples with exit code, tests, coverage and mutation score, measured in your last run. Then what you decided on your own, what you could not verify and why, and every `@status/validated` you placed or removed. State plainly anything that still differs between the stacks.
