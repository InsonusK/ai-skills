---
name: registry-component-name-component-ts
description: Conflict Detection result for the `component-name-component-ts` element in the plateau-online-monolith plateau
tags:
  - concern/architecture
  - stack/typescript
  - element/component-name-component-ts
---

# Element
`{component-name}.component.ts` — the generic feature-lib component in `libs/{feature}/feature/src/lib/{component-name}/`. (Not the design-system's `ds-{component}.component.ts` — that is `element/ds-component-ts` in a different catalog, see [delta-conflict-analysis.md](skills/angular/architecture/v3.1/delta-conflict-analysis.md#pre-analysis-fixes-applied-to-the-catalog).)

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]] (`.extend` — `FormComponent/{component-name}.component.ts.extend` — Signal Forms wiring: `form()`/`FieldTree`, `submitForm()` calling the feature Facade)
- [[skills/angular/architecture/v3.1/solutions/solution-state-tiering.skill/solution-state-tiering.skill.md|solution-state-tiering]] (`.extend` — `LocalState/{component-name}.component.ts.extend` — a component-tier `signal()` field for view-only state)

# Classification
`FMN` — Constraint `F` (both are common baseline conventions; the Feature Model has no VP row and therefore no constraint linking them). Category `M` (both change the component class body). Kind `N` (independent): `solution-forms` adds form-model wiring and a submit handler; `solution-state-tiering` adds an unrelated `signal()` field. Member-disjoint — neither touches a statement the other wrote. The granularity is the method: no shared method.

# Ordering
`source: ordering-only` — no Feature-Model constraint. Both are common, always co-present in every monolith plateau from the baseline. The order is irrelevant (independent additions); recorded here only so an author knows both may extend the same file.

# Resolution
**Canonical — resolved by convention, no resolver solution.** Each solution's `.extend` adds a distinct member. The `plateau-online-monolith` example demonstrates it: `order-form.component.ts` carries the Signal Forms `[(ngModel)]` form and its `submit()` handler (from `solution-forms`); a component that also needs view-only local state would add a `signal()` per `solution-state-tiering` without either touching the other's code.
