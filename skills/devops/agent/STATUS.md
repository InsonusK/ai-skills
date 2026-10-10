# skills/devops — status

Branch `devops-rework`, worktree `.ai-worktree/devops-rework`, cut from `develop` at `aa241fc7`.

**Now:** W8 waits for the owner: F2, F10, F11 in `DECISIONS.md`, and a way to read the results of a GitHub run (`gh` is not logged in in this container). Everything else is done; `bash skills/devops/agent/check.sh` is green (with `actionlint` on `PATH` and `npm install` run in `skills/devops/agent/`).

## Waves

| Wave | Produces | Class | State |
| --- | --- | --- | --- |
| W0 | `INVARIANTS.md`, `DECISIONS.md`, this file | new | reviewed 2026-10-10 |
| W1 | `agent/check.sh` (links, the §1 check, every stack has every extension, no reference to a removed skill), `agent/fixtures.sh` | new | done |
| W2 | `core/devops-ci-orchestration`; `core/devops-project-version` with `tools/version/` + `-in-go` — the sample, run by `fixtures.sh` | new | done |
| W3 | `devops-project-version-in-{python,typescript,dotnet}`; Angular uses the TypeScript one; `devops-service-deploy` moved to `deploy/` | new, from `check-version-in-{stack}` | done |
| W4 | `core/devops-ci-changes` + `-in-{go,python,typescript,dotnet,angular}` | new, from `check-changes-in-{stack}`; fixes `CONTEXT.md` gaps 2–4; patterns run by `agent/changes-fixtures.mjs` | done |
| W5 | `core/devops-ci-toolchain` + `-in-{go,python,typescript,dotnet}`; `assemble-workflow.sh`; `workflows/devops-github-wf-pull-request` with `templates/pull-request.yml` | new / rewrite | done |
| W6 | `workflows/devops-github-wf-release` with `templates/release.yml`; `core/devops-package-publish` + `-in-{python,typescript,dotnet}`; `devops-app-release-in-go` | rewrite, merges four workflow skills | done |
| W7 | the replaced skills removed (`agent/removed-skills.txt`); rule and ADR in `skill-design` for the `skills/devops/` layout; no link to a removed skill is left in `skills/` | move / delete | done |
| W8 | ground truth. Done: `fixtures.sh` (11 version cases x 4 stacks), `changes-fixtures.mjs` (path cases x 5 stacks), `actionlint` on every assembled variant, `test/devops/run-local.sh` on the Go sample. Not done: a run on GitHub | — | waits for the owner |

## Unproved until a GitHub run

- `dorny/paths-filter` with `base: ${{ github.ref }}` and `predicate-quantifier: every` — the patterns are tested with its matcher, the action itself is not.
- Every job of `release.yml` after the tests: the image push, the three package jobs, the Go binaries, Pages, the Release.
- Starting test services from `.devcontainer/docker-compose.yml`; no sample has a database.
- The Python, TypeScript, and .NET samples: only `tools/version/` and the path patterns ran for them.
