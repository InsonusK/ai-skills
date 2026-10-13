---
description: Select the library TestBed target and its browser host.
element_kind: repository
change_kind: extend
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-library
  - element/testing-repository
  - stack/typescript
  - framework/angular
  - concern/testing
---

# Goal
- Refine runner selection while inheriting all spec and result rules.

# Mutations
Apply the [base repository setup](../../solution-conformance-testing-in-angular.skill/Implementation/Repository.extend.md). No additional plateau or alternate runner is applied.

Copy [components.sh](../assets/tools/testing/kinds/components.sh) over `tools/testing/kinds/components.sh`; replace the identifier `linkcheck` with the actual library project. Inherit `ui.sh` and `angular-results.mjs` unchanged from the base and `unit.sh`/`mutation.sh` unchanged from its TypeScript parent.

Merge the public-entry `paths` alias from [tsconfig.json](../assets/tsconfig.json) into the existing root TypeScript configuration; use relative (`./`) paths. The demo imports that public entry point.

Use [angular.json](../assets/angular.json), [tsconfig.spec.json](../assets/tsconfig.spec.json) and [Vitest config](../assets/vitest.components.config.mts) for the library test target. Its `buildTarget` names the demo application's development build, since the Angular unit-test builder uses application compilation rather than ng-packagr. Match project identifiers and source paths to the repository; preserve the native JSON and coverage output paths.

Instantiate the inherited Playwright template with the library's source root (`projects/linkcheck/src`) and `npm run start --`, with `start` running `ng serve demo`. Keep its snapshot path expressions and dynamic port intact. README badges remain the five inherited badges.

# Rule
## MUST
### Select the library explicitly
Pass the library project to `ng test` and configure a demo application `buildTarget`.
- Risk: adding the host makes the base's implicit single-project command ambiguous or attempts ng-packagr compilation as an application.
- Fix: use the supplied target selection, keeping the TestBed specs in the library.
