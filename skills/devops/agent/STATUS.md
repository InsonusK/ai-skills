# skills/devops — status

Branch `devops-rework`, worktree `.ai-worktree/devops-rework`, cut from `develop` at `aa241fc7`.

**Now:** done, apart from what only a GitHub run shows. `bash skills/devops/agent/check.sh` is green (with `actionlint` on `PATH` and `npm install` run in `skills/devops/agent/`). The pull-request workflow ran green on GitHub on 2026-10-10 (pull request `devops-sample-change` into `develop-devops`, confirmed by the owner): `code` detected, `version-check` skipped, README check and the unit kind passed, the mutation kind skipped itself, the report job passed. The release workflow is not tried on GitHub by the owner's decision.

## Waves

| Wave | Produces | Class | State |
| --- | --- | --- | --- |
| W0 | `INVARIANTS.md`, `DECISIONS.md`, this file | new | reviewed 2026-10-10 |
| W1 | `agent/check.sh` (links, the §1 check, every stack has every extension, no reference to a removed skill), `agent/fixtures.sh` | new | done |
| W2 | `core/devops-ci-orchestration`; `core/devops-project-version` with `tools/version/` + `-in-go` — the sample, run by `fixtures.sh` | new | done |
| W3 | `devops-project-version-in-{python,typescript,dotnet}`; Angular uses the TypeScript one; `devops-service-deploy` moved to `deploy/` | new, from `check-version-in-{stack}` | done |
| W4 | `core/devops-ci-changes` + `-in-{go,python,typescript,dotnet,angular}` | new, from `check-changes-in-{stack}`; fixes `CONTEXT.md` gaps 2–4; patterns run by `agent/changes-fixtures.mjs` | done |
| W5 | `core/devops-ci-toolchain` + `-in-{go,python,typescript,dotnet}`; `core/devops-github-wf-pull-request` with `pull-request.yml` as an asset | new / rewrite | done |
| W6 | `core/devops-github-wf-release` with `release.yml` as an asset; six release-action skills `devops-release-*` | rewrite, merges four workflow skills | done |
| W7 | the replaced skills removed (`agent/removed-skills.txt`); rule and ADR in `skill-design` for the `skills/devops/` layout; no link to a removed skill is left in `skills/` | move / delete | done |
| W8 | ground truth. Done: `fixtures.sh` (11 version cases x 4 stacks), `changes-fixtures.mjs` (path cases x 5 stacks), `actionlint` on every assembled variant, `test/devops/run-local.sh` on the Go sample. A GitHub run of `pull-request.yml` on the Go sample: green | — | done |

## Unproved until a GitHub run

- `check-changes` on a push (`base: ${{ github.ref }}`) and for categories other than `code`; on a pull request with a code change it ran green.
- `make version-check` inside a workflow: the sample's pull request did not go into `master`.
- Every job of `release.yml` after the tests: `deliver` with any release action, Pages, the Release. Of the release actions only the naming steps of the Docker and Go ones and the Go build ran locally.
- `delivery-check` of `pull-request.yml` — added after the green run of 2026-10-10.
- Starting test services from `.devcontainer/docker-compose.yml`; no sample has a database.
- The Python, TypeScript, and .NET samples: only `tools/version/` and the path patterns ran for them.
