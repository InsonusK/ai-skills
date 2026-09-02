---
description: Baseline structure for the design system's own repository — a plain Angular CLI multi-project workspace with the publishable library, a demo app, ng-packagr build, and Changesets-based releases
element_kind: repository
change_kind: create
tags:
  - solution/design-system-structure
  - element/design-system-repository
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
| /projects/demo | A plain Angular application, not published, importing `design-system` as a regular dependency and rendering example usage of every component — the project's answer to component preview/visual review, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling]]. |
| /.changeset | Changesets configuration and pending changeset files, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/release-versioning-strategy.md|release-versioning-strategy]]. |

# Rules

## MUST
- The repository MUST be a plain Angular CLI multi-project workspace, not an Nx workspace, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/design-system-workspace-tooling.md|design-system-workspace-tooling]].
  - Risk: Nx's benefits (affected builds, enforced boundaries, federation generators) do not meaningfully apply to a two-project repository, so adopting it imports tooling complexity disproportionate to the repository's actual size.
  - Fix: create a plain Angular CLI multi-project workspace with `projects/design-system` (library) and `projects/demo` (application).
- The library project MUST be built with ng-packagr, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/library-build-tooling.md|library-build-tooling]] — no custom Vite/Rollup build is used.
  - Risk: a custom Vite/Rollup build cannot guarantee Angular Package Format compliance and Ivy partial compilation, so consumers on a range of Angular versions may be unable to consume the library.
  - Fix: build the library with ng-packagr, the Angular-team-maintained tool for producing Angular Package Format-compliant output.
- Every pull request that changes the published library's public API or behavior MUST include a changeset file, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/release-versioning-strategy.md|release-versioning-strategy]].
  - Risk: without a per-PR changeset, version bump classification becomes a release-time judgment call, so a breaking change can reach multiple independent consumer teams under a non-major version.
  - Fix: add a changeset file describing the change and its intended bump to every PR touching the library's public surface, and let CI fail the PR when it is missing.
- Never publish the `demo` project to npm — only `design-system` is a publishable artifact.
  - Risk: an internal preview application becomes a published package, confusing consumers about which artifact is the library and shipping code that was never meant for reuse.
  - Fix: publish only the `design-system` library; keep `demo` as a workspace-local, unpublished application.
- Never publish changes to the library without a changeset.
  - Risk: the release tooling has nothing to base the next version bump/changelog entry on, reintroducing the manual-discipline risk [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/release-versioning-strategy.md|release-versioning-strategy]] was chosen to avoid.
  - Fix: include a changeset file in every PR touching the library's public surface.
- Never reach for Storybook again "just for this one component" after this decision.
  - Risk: reintroduces the exact friction this repository's ADR already identified from direct prior experience, and fragments preview tooling across two different approaches.
  - Fix: add the component's preview to the existing `demo` app, consistent with the rest of the library.

## SHOULD
- Every new component added to the library SHOULD get a corresponding example page in `demo`, so visual review stays available for every shipped component.

# Unittest TestCases

- [ ] WHEN the library is built THEN
  - [ ] the output conforms to the Angular Package Format (Ivy partial compilation)
- [ ] WHEN a PR modifies the library's public API THEN
  - [ ] CI fails if no changeset file is present
