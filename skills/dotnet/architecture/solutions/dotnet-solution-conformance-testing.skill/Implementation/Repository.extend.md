---
description: Add the PR-gate/trunk-gate CI jobs and README badges for the Cucumber/coverage/mutation quality gate
element_kind: repository
change_kind: extend
---

# Structure

## Project Structure
```
/.github
  /workflows
    ci.yml
/{Module}
  /{Module}.Tests
    Rules/{Rule}.feature
    StepDefinitions/{Rule}Steps.cs
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /.github/workflows | ci.yml | PR-gate and trunk-gate jobs |
| / | README.md | Coverage badge and mutation-score badge |

# Rules

## MUST
- `ci.yml` must define two separate jobs: a PR-gate job triggered on `pull_request` targeting `master`, and a trunk-gate job triggered on `push` to `master`.
- The PR-gate job must run `dotnet test` and `dotnet-stryker` in its "since" mode (comparing against `master`, consult Stryker.NET's current docs for the exact flag/config key), so only mutants in changed code are evaluated, and fail the check on a red test or a new surviving mutant.
- The trunk-gate job must run `dotnet test` with coverlet across the whole module, `dotnet-stryker` without diff-scoping across the whole module, publish both reports as workflow artifacts, and regenerate the two README badges from this run.
- `README.md` must display a coverage badge and a mutation-score badge, both sourced from the latest trunk-gate artifact.

## MUST NOT
- The PR-gate job must not run `dotnet-stryker` without diff-scoping — full-module mutation testing belongs to the trunk gate only.
- The trunk-gate job must not block anything retroactively; a red trunk gate is fixed forward, not reverted through history rewriting.

# Anti-patterns
- **Running the same `dotnet-stryker` invocation in both jobs**
  - Consequence: PR feedback becomes as slow as the trunk gate, and the team eventually disables mutation testing on PRs under deadline pressure.
  - Instead: enable Stryker.NET's "since" mode against `master` (or the project's default branch) in the PR-gate job only; the trunk-gate job runs the full, unscoped command.
- **Badge sourced from a PR run**
  - Consequence: the README shows numbers that were never actually merged, misleading anyone who checks it against `master`.
  - Instead: regenerate badge data only inside the trunk-gate job, after the merge.

# Unittest TestCases
- [ ] WHEN a PR touches `{Module}/{Rule}.cs` and a mutant in it survives THEN the PR-gate job fails.
- [ ] WHEN `master` is updated THEN the trunk-gate job runs full coverage and full mutation testing and updates both README badges.
