---
description: Baseline structure for the design system's own repository — a plain Angular CLI multi-project workspace with the publishable library, a demo app, ng-packagr build, and Changesets-based releases
element_kind: repository
change_kind: create
---

# Structure

## Workspace Structure

```
/design-system-repo
  angular.json
  /projects
    /design-system         <- publishable library (ng-packagr)
      /src/lib
      ng-package.json
      package.json
    /demo                  <- Angular application, component preview (not published)
      /src
  /.changeset
  package.json
```

## Directory and project skills

| Directory | Description |
| ---------- | ----------- |
| /projects/design-system | The publishable library project. Built with ng-packagr, producing Ivy partial compilation output per the Angular Package Format. This is the only project published to npm. |
| /projects/demo | A plain Angular application, not published, importing `design-system` as a regular dependency and rendering example usage of every component — the project's answer to component preview/visual review, per [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/component-preview-tooling]]. |
| /.changeset | Changesets configuration and pending changeset files, per [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/release-versioning-strategy]]. |

# Rules

## MUST
- The repository MUST be a plain Angular CLI multi-project workspace, not an Nx workspace, per [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/design-system-workspace-tooling]].
- The library project MUST be built with ng-packagr, per [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/library-build-tooling]] — no custom Vite/Rollup build is used.
- Every pull request that changes the published library's public API or behavior MUST include a changeset file, per [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/release-versioning-strategy]].
- The `demo` project MUST NOT be published to npm — only `design-system` is a publishable artifact.

## SHOULD
- Every new component added to the library SHOULD get a corresponding example page in `demo`, so visual review stays available for every shipped component.

# Anti-patterns

- **Publishing changes to the library without a changeset**
  - Consequence: the release tooling has nothing to base the next version bump/changelog entry on, reintroducing the manual-discipline risk [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/release-versioning-strategy]] was chosen to avoid
  - Instead: every PR touching the library's public surface includes a changeset

- **Reaching for Storybook again "just for this one component" after this decision**
  - Consequence: reintroduces the exact friction this repository's ADR already identified from direct prior experience, and fragments preview tooling across two different approaches
  - Instead: add the component's preview to the existing `demo` app, consistent with the rest of the library

# Unittest TestCases

- [ ] WHEN the library is built THEN
  - [ ] the output conforms to the Angular Package Format (Ivy partial compilation)
- [ ] WHEN a PR modifies the library's public API THEN
  - [ ] CI fails if no changeset file is present
