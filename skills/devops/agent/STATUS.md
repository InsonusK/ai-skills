# skills/devops — status

Branch `devops-rework`, worktree `.ai-worktree/devops-rework`, cut from `develop` at `aa241fc7`.

**Now:** waiting for the owner's review of `INVARIANTS.md` and the ⚠️ entries of `DECISIONS.md`. Nothing outside `skills/devops/agent/` is changed.

## Waves

| Wave | Produces | Class | State |
| --- | --- | --- | --- |
| W0 | `INVARIANTS.md`, `DECISIONS.md`, this file | new | draft, in review |
| W1 | `agent/check.sh` (links, `run:` rule, every stack has every extension, no reference to a removed skill), `agent/fixtures.sh` | new | — |
| W2 | `core/devops-ci-orchestration` with `tools/ci/` assets; `core/devops-project-version` + `-in-go` — the sample, run by `fixtures.sh` | new | — |
| W3 | `devops-project-version-in-{python,typescript,dotnet,angular}` | new, from `check-version-in-{stack}` | — |
| W4 | `core/devops-ci-changes` + `-in-{go,python,typescript,dotnet,angular}` | new, from `check-changes-in-{stack}`; fixes `CONTEXT.md` gaps 2–4 | — |
| W5 | `workflows/devops-github-wf-pull-request` | rewrite | — |
| W6 | `workflows/devops-github-wf-release`; `core/devops-package-publish` + three extensions; `devops-app-release-in-go` | rewrite, merges four workflow skills | — |
| W7 | remove the replaced skills, move `devops-service-deploy` to `deploy/`, repoint every link in `skills/` (testing skills and plateau catalogs name the old workflow skills) | move / delete | — |
| W8 | ground truth: fixtures green, `actionlint`, a GitHub run if a repository is given; hand-off | — | — |

## Replaced skills (removed in W7)

`skills/devops/workflows/`: `-docker-release-publish`, `-release-info-publish`, `-release-test-report`, `-stack-lib-release-publish`.
`skills/{go,python,typescript,dotnet}/devops/`: `devops-github-action-check-changes-in-*`, `devops-github-action-check-version-in-*`, `devops-github-wf-stack-lib-release-publish-in-{python,typescript,dotnet}`, `devops-github-wf-release-info-publish-in-go`.
