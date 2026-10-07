# Status

Worktree `.ai-worktree/skills-testing`, branch `skills-testing` (base `develop`). Check: `bash skills/testing/agent/check.sh`.

| Wave | Content | State |
| --- | --- | --- |
| W0 | Anchor document, decisions log | done `a4e5d442` |
| W1 | Move: 21 skills from `skills/common-workflow/test/` and `skills/{stack}/test/` into `skills/testing/{skill-name}/` (`moves.tsv`); renames `cucmber`→`cucumber`, `no-test-theater-{stack}`→`-in-{stack}`, `dotnet-unittest`→`unittest-in-dotnet`, `testing-strategy`→`testing-strategy-in-dotnet`; every reference in the repository rewritten; 4 links out of `skills/testing/` turned into plain names; `skill-design` rule "Keep testing skills together" + ADR; `validation-config.yaml` exclude pattern; `check.sh` | done |
| W2 | DevOps contract (INVARIANTS §5) in `solution-conformance-testing` and its four stack skills, with its ADR | blocked — see below |
| W3 | DevOps workflow skills use only the contract | after W2 |
| W4 | Layout by convention and tests of pluggable modules as testing skills; plateau `*.Tests` structure skills removed; base plateaus name the testing skills | after the forks below |

## Waiting on the owner

- ⚠️ **Go: the testing skill depends on a catalog solution.** `solution-conformance-testing-in-go` extends the `Makefile` that `solution-go-repository-structure` creates (3 links). Options: the testing skill creates the `Makefile` when there is none and extends it otherwise (no dependency; the catalog solution keeps adding its own targets) — or repository structure moves into a stack skill outside the catalog.
- ⚠️ **Python: the testing skill builds on `solution-test`** — the Python catalog's unit-test layout solution (2 files, 6 links). It is test layout by convention, which INVARIANTS §1 places in `skills/testing/`; moving it changes `plateau-python-cli`'s `created_by`.

Both are listed in `isolation-exceptions.tsv`; `check.sh` passes with exactly these and fails on any new one.

## Blocked: no runtime in this environment

W2 changes `make` targets, scripts and report layout that about 240 files carry, including the runnable plateau examples (Go, dotnet, Angular). Neither Go, .NET, Node, Python nor Docker is installed here, so the changed targets cannot be run. W2 needs an environment where the examples build.

## Not changed

- `.validation/*-log.yaml` still key the moved skills by their old paths — the files are written only by `validation_queue.py`, which cannot run here. Re-run it; the one real date lost is `no-test-theater-in-angular` (20260909), the rest were never validated.
- ADRs and catalog `agent/DECISIONS.md` journals keep the old directory names in prose where they describe a past decision.
- Catalog testing solutions stay where they are until W4: dotnet `solution-dotnet-conformance-testing`, `solution-cecil-architecture-tests`; python `solution-test`; angular `solution-app-testing`, `solution-ui-testing`, `solution-design-system-ui-testing`.
- 140 broken links elsewhere in the repository existed before this branch and are untouched.
