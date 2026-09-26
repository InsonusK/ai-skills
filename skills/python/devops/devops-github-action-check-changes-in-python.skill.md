---
name: devops-github-action-check-changes-in-python
description: Python-specific implementation of the check-changes reusable composite action — dorny/paths-filter patterns for a Python project's src/test/features/Dockerfile/docs layout
whenToUse: when creating or updating `.github/actions/check-changes/action.yml` in a Python project
updated: 20260915
tags:
  - stack/python
  - concern/ci
  - github-actions

---

# Scope
This skill adds Python-specific filter patterns on top of the `check-changes` composite action consumed by [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- The filter patterns reflect the Python layout used by [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md) (`src/`, `test/`) and [solution-conformance-testing-in-python](skills/python/test/solution-conformance-testing-in-python.skill/solution-conformance-testing-in-python.skill.md) (`features/` for Gherkin) — never a generic guess at Python project structure.

# Rule

## MUST

### Implement action.yml with Python's real paths
Create `.github/actions/check-changes/action.yml` as a composite action wrapping `dorny/paths-filter@v3`, with Python's actual source/test/docs paths.
```yaml
name: check-changes
description: Detect which categories of files changed
outputs:
  code:
    value: ${{ steps.filter.outputs.code }}
  test:
    value: ${{ steps.filter.outputs.test }}
  workflow:
    value: ${{ steps.filter.outputs.workflow }}
  docker:
    value: ${{ steps.filter.outputs.docker }}
  docs:
    value: ${{ steps.filter.outputs.docs }}
runs:
  using: composite
  steps:
    - uses: dorny/paths-filter@v3
      id: filter
      with:
        filters: |
          code:
            - 'src/**'
            - 'pyproject.toml'
          test:
            - 'test/**'
            - 'features/**'
          workflow:
            - '.github/workflows/**'
            - '.github/actions/**'
          docker:
            - 'Dockerfile'
          docs:
            - 'docs/**'
            - '*.md'
```
- Violation: filtering on `tests/**` (plural) or omitting `features/**`, which drifts from `test/`/`features/` — the paths [solution-test](skills/python/architecture/solutions/solution-test.skill/solution-test.skill.md) and [solution-conformance-testing-in-python](skills/python/test/solution-conformance-testing-in-python.skill/solution-conformance-testing-in-python.skill.md) actually create.
- Risk: a change to a Gherkin scenario in `features/` silently fails to trigger `unit-test`/`mutation-test`, so a broken scenario merges undetected.
- Fix: match the paths those two skills actually produce, not an assumed convention.

### Composite outputs match the consumer's contract
Expose exactly `code`, `test`, `workflow`, `docker`, `docs` as this action's outputs, named identically to what [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]] read from `needs.changes.outputs.*`.
- Risk: a renamed or missing output breaks every consumer workflow's `if:` conditions silently (an unset output evaluates as an empty string, which is falsy — the job just never runs, with no error).
- Fix: keep the output names exactly as listed above.

# Check list
- [ ] `.github/actions/check-changes/action.yml` exists and wraps `dorny/paths-filter@v3`.
- [ ] `code`/`test`/`workflow`/`docker`/`docs` filters match `src/`/`pyproject.toml`, `test/`+`features/`, `.github/workflows/`+`.github/actions/`, `Dockerfile`, `docs/`+`*.md` respectively.
- [ ] The action's outputs are named `code`, `test`, `workflow`, `docker`, `docs` — matching every consumer workflow's `needs.changes.outputs.*` references.
