---
name: devops-github-action-check-changes-in-python
description: Python-specific implementation of the check-changes reusable composite action — dorny/paths-filter patterns for a Python project's src/test/features/Dockerfile/docs layout
whenToUse: when creating or updating `.github/actions/check-changes/action.yml` in a Python project
updated: 20261008
tags:
  - stack/python
  - concern/ci
  - github-actions

---

# Scope
This skill adds Python-specific filter patterns on top of the `check-changes` composite action consumed by [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- The filter patterns reflect the Python layout [cucumber-testing-in-python](skills/testing/python/cucumber-testing-in-python.skill/cucumber-testing-in-python.skill.md) defines — features in `{package}/features/`, tests in `{package}/test/`, both below `src/` — never a generic guess at where tests live.

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
        # every: a file counts for a filter only when it matches all of its patterns - what
        # lets `code` leave out the tests that sit below src/. One filter that names several
        # places therefore lists them inside braces.
        predicate-quantifier: every
        filters: |
          code:
            - '{src/**,pyproject.toml}'
            - '!src/**/test/**'
            - '!src/**/features/**'
          test:
            - 'src/**/{test,features}/**'
          workflow:
            - '.github/{workflows,actions}/**'
          docker:
            - 'Dockerfile'
          docs:
            - '{docs/**,*.md}'
```
- Violation: `code: - 'src/**'` with no negated pattern; a `test` filter on root-level `test/**` and `features/**`.
- Risk: tests sit below `src/`, so without the negations a change to one scenario counts as a code change — the pull-request workflow then demands a version bump for it; a filter on root-level folders matches nothing, and a broken scenario merges with no test job run.
- Fix: keep `predicate-quantifier: every` with the negated patterns, matching the paths [cucumber-testing-in-python](skills/testing/python/cucumber-testing-in-python.skill/cucumber-testing-in-python.skill.md) produces.

### Composite outputs match the consumer's contract
Expose exactly `code`, `test`, `workflow`, `docker`, `docs` as this action's outputs, named identically to what [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]] read from `needs.changes.outputs.*`.
- Risk: a renamed or missing output breaks every consumer workflow's `if:` conditions silently (an unset output evaluates as an empty string, which is falsy — the job just never runs, with no error).
- Fix: keep the output names exactly as listed above.

# Check list
- [ ] `.github/actions/check-changes/action.yml` exists and wraps `dorny/paths-filter@v3`.
- [ ] `code` matches `src/` and `pyproject.toml` without `src/**/test/` and `src/**/features/`; `test` matches exactly those two; `workflow`, `docker`, `docs` match `.github/workflows/`+`.github/actions/`, `Dockerfile`, `docs/`+`*.md`.
- [ ] A pull request that changes only a `.feature` file sets `test` and leaves `code` unset.
- [ ] The action's outputs are named `code`, `test`, `workflow`, `docker`, `docs` — matching every consumer workflow's `needs.changes.outputs.*` references.
