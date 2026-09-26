---
name: devops-github-action-check-changes-in-typescript
description: TypeScript-specific implementation of the check-changes reusable composite action — dorny/paths-filter patterns for a TypeScript package's src/test/features/Dockerfile/docs layout
whenToUse: when creating or updating `.github/actions/check-changes/action.yml` in a TypeScript project
updated: 20260915
tags:
  - stack/typescript
  - concern/ci
  - github-actions

---

# Scope
This skill adds TypeScript-specific filter patterns on top of the `check-changes` composite action consumed by [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- The filter patterns reflect [solution-conformance-testing-in-typescript](skills/typescript/test/solution-conformance-testing-in-typescript.skill/solution-conformance-testing-in-typescript.skill.md)'s layout (`src/`, `features/` for Cucumber, `*.spec.ts` for Vitest) — never a generic guess at TypeScript project structure.

# Rule

## MUST

### Implement action.yml with TypeScript's real paths
Create `.github/actions/check-changes/action.yml` as a composite action wrapping `dorny/paths-filter@v3`, with TypeScript's actual source/test/docs paths.
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
            - 'package.json'
            - 'tsconfig*.json'
          test:
            - 'features/**'
            - '**/*.spec.ts'
            - '**/*.test.ts'
          workflow:
            - '.github/workflows/**'
            - '.github/actions/**'
          docker:
            - 'Dockerfile'
          docs:
            - 'docs/**'
            - '*.md'
```
- Violation: filtering only `src/**` for `test`, missing `features/**` (Cucumber) or `*.spec.ts`/`*.test.ts` (Vitest) — the two suites [solution-conformance-testing-in-typescript](skills/typescript/test/solution-conformance-testing-in-typescript.skill/solution-conformance-testing-in-typescript.skill.md) actually runs.
- Risk: a change to a Gherkin scenario or a Vitest spec silently fails to trigger `unit-test`/`mutation-test`, so a broken scenario merges undetected.
- Fix: match the paths that skill actually creates, not an assumed convention.

### Composite outputs match the consumer's contract
Expose exactly `code`, `test`, `workflow`, `docker`, `docs` as this action's outputs.
- Risk: a renamed or missing output silently breaks every consumer workflow's `if:` conditions (an unset output is falsy, so the job just never runs).
- Fix: keep the output names exactly as listed above.

# Check list
- [ ] `.github/actions/check-changes/action.yml` exists and wraps `dorny/paths-filter@v3`.
- [ ] `code`/`test`/`workflow`/`docker`/`docs` filters match `src/`+`package.json`+`tsconfig*.json`, `features/**`+`*.spec.ts`+`*.test.ts`, `.github/workflows/`+`.github/actions/`, `Dockerfile`, `docs/`+`*.md` respectively.
- [ ] The action's outputs are named `code`, `test`, `workflow`, `docker`, `docs`.
