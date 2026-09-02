---
description: Bump the workspace's minimum Angular version to 22+ to unlock stable Signal Forms, and add the convention for where form field definitions live inside a feature
element_kind: repository
change_kind: extend
tags:
  - solution/forms
  - element/repository
---

# Structure

No new directories. This extension adds a version requirement and a placement convention on top of [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create]].

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| libs/{feature}/feature/src/lib/**/*.form.ts | Optional, used only when a form's field schema/validators are non-trivial enough to extract from the component — see [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/Implementation/FormComponent/{component-name}.component.ts.extend]] for when to extract vs. keep inline |

# NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| @angular/core, @angular/forms | >= 22.0 | Minimum version required for Signal Forms to be available as stable (see [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/adr/forms-approach.md|forms-approach]]) |

# Rules

## MUST
- The workspace MUST run Angular >= 22 before this solution's rules apply — Signal Forms is only stable from that version onward.
- New forms MUST use Signal Forms (`@angular/forms/signals`) by default, per [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/adr/forms-approach.md|forms-approach]].

# Unittest TestCases

- [ ] WHEN a new form component is added to the codebase THEN
  - [ ] it is built with `form()`/`FieldTree` from `@angular/forms/signals`, not `FormGroup`/`FormControl`

## SHOULD
- Avoid existing Reactive Forms code SHOULD NOT be migrated to Signal Forms purely for the sake of consistency — only migrate a form when it is already being substantially rewritten for other reasons.
- **Starting a brand-new form with Reactive Forms "because that's what the rest of the codebase still has"** — Consequence: perpetuates the older pattern indefinitely and never captures the Signal Forms benefits this solution exists to adopt — Instead: every new form starts with Signal Forms; only an existing, untouched Reactive Forms form is left as-is
- **Mass-migrating all existing Reactive Forms to Signal Forms in one pass** — Consequence: large, high-risk changeset with no functional benefit to users, and a real chance of subtly breaking complex existing forms — Instead: migrate opportunistically, only when a form is already being substantially reworked
