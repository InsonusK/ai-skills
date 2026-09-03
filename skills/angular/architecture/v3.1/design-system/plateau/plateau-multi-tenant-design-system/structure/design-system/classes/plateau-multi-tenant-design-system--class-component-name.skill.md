---
name: plateau-multi-tenant-design-system--class-component-name
description: Generic pattern for authoring any ds-* component — own ds-* selector, signal-based API designed around real usage axes, per-component delegate-to-Material-or-custom decision, full Material encapsulation (source AND built .d.ts), four test layers — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when adding or editing a ds-* component under projects/design-system/src/lib/{component}/, deciding its API shape, or checking Material never leaks into its public surface
plateau: multi-tenant-design-system
artifact_type: component
version: 20260903200000
tags:
  - skill/template/class
  - plateau/multi-tenant-design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
---

> Not tied to one concrete component — every component added to the library follows this. Worked examples below: `DsButtonComponent` (delegates to `matButton` internally) and `DsStatusChipComponent` (fully custom, `--ds-*` tokens).

# Goal

- Give application developers a component API designed around real usage needs, with zero required Angular Material knowledge
- Keep the internal implementation free to delegate to Material or be fully custom, per this component's own real requirements
- Test the component purely on its own `input()`/`output()`/`model()` surface — nothing to fake — plus the visual / style / a11y regressions a behavioural test structurally cannot catch

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]

# Core Principles

- Apply ONE plateau template per component
- `input()`, `output()`, `model()` exclusively — no decorators, no `EventEmitter`
- Own `ds-*` selector; API designed around real usage axes, never a passthrough of an underlying Material component's inputs
- **No Angular Material type, selector, or enum on the public surface** — and "public surface" means the *built* `dist/design-system/types/*.d.ts`, not only the `.ts` source
- A behavioural test provides only the component's own inputs — a pure `ds-*` component injects no dependency to fake

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/adr/component-encapsulation-strategy.md|component-encapsulation-strategy]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Component | `Ds{ComponentName}Component` | `DsButtonComponent` | `ds-{component-name}.component.ts` | `ds-button.component.ts` |
| Selector | `ds-{component-name}` | `ds-button` | — | — |

# Implementation

**Delegating example** — a button whose API is organized around real usage axes, not Material's `appearance` values. It renders `matButton` internally.

```typescript
// Skill: class-component-name
// Plateau: design-system
// Version: 20260903200000
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButton } from '@angular/material/button';

export type DsButtonVariant = 'solid' | 'outline' | 'text';
export type DsButtonTone = 'primary' | 'danger';

// Local literal type — NEVER `MatButtonAppearance` imported from Material, so no
// Material type reaches the generated .d.ts (ng-packagr emits `protected` members).
type MatAppearance = 'filled' | 'outlined' | 'text';
const VARIANT_TO_MAT: Record<DsButtonVariant, MatAppearance> = {
  solid: 'filled',
  outline: 'outlined',
  text: 'text',
};

@Component({
  selector: 'ds-button',
  imports: [MatButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button [matButton]="matAppearance()" data-testid="ds-button"
            [class.ds-button--danger]="tone() === 'danger'"
            [disabled]="disabled()" (click)="pressed.emit()">
      <ng-content>{{ label() }}</ng-content>
    </button>
  `,
})
export class DsButtonComponent {
  readonly label = input('');
  readonly variant = input<DsButtonVariant>('solid');
  readonly tone = input<DsButtonTone>('primary');
  readonly disabled = input(false);
  readonly pressed = output<void>(); // named for intent, not Material's `click`
  protected readonly matAppearance = computed(() => VARIANT_TO_MAT[this.variant()]);
}
```

**Fully-custom example** — a workflow status chip; Material models no such concept, so it is built custom and consumes `--ds-color-status-*` (with `--mat-sys-*` for typography):

```typescript
@Component({
  selector: 'ds-status-chip',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="ds-status-chip" [class]="'ds-status-chip--' + status()"
                   data-testid="ds-status-chip" role="status">{{ resolvedLabel() }}</span>`,
  styles: `
    .ds-status-chip { padding: var(--ds-spacing-xs) var(--ds-spacing-sm);
      border-radius: var(--ds-radius-lg); font: var(--mat-sys-label-large); }
    .ds-status-chip--in-progress { background: var(--ds-color-status-in-progress); }
  `,
})
export class DsStatusChipComponent {
  readonly status = input.required<DsStatus>();
  readonly label = input('');
  protected readonly resolvedLabel = computed(() => this.label() || STATUS_LABEL[this.status()]);
}
```

**Form control** — a `ds-*` component participating in a form implements `ControlValueAccessor` so it is compatible with Signal Forms' `formField` binding:

```typescript
@Component({
  selector: 'ds-text-field',
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: DsTextFieldComponent, multi: true }],
  /* ... */
})
export class DsTextFieldComponent implements ControlValueAccessor {
  // writeValue / registerOnChange / registerOnTouched / setDisabledState
}
```

Every component also gets a `spec/preview/{component-name}.preview.ts` (imported by `projects/demo`), and `spec/{component-name}.{component,visual,style-snapshot,a11y}.spec.ts`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|Testing/{component-name}.component.spec.ts.create]]

# Rules

## MUST
- The component's inputs/outputs are named around real usage concepts (`variant`/`tone`/`pressed`), never mirrored from an underlying Material component's own input names.
- No Material type/enum leaks into the component's own input/output types — **or into the built `.d.ts`**. Internal Material-mapping helpers use local literal types (`'filled' | 'outlined' | 'text'`), never `MatButtonAppearance` and friends.
- A form control implements `ControlValueAccessor`.
- A behavioural test provides nothing beyond the component's own inputs — no provider/mock for a dependency the component doesn't inject.
- The component has a `projects/demo` preview page per meaningfully distinct state, and passing visual, style-snapshot, and a11y specs.

## SHOULD
- Default to delegating to Angular Material internally where it fully satisfies the requirement; build fully custom only for a specific, identified gap (a large-dataset control needing different performance than Material's; a domain concept — like workflow status — Material does not model).

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]


- **Naming an input identically to Material's own corresponding input, with the same enum of values**
  - Consequence: mirrors Material's categorization closely enough that any change to Material's API forces a parallel change here — the encapsulation exists in name only
  - Instead: design the input around this application's real usage
- **Typing an internal Material-mapping helper with a Material-exported type "since it's only `protected`"**
  - Consequence: ng-packagr emits `protected` members into `types/*.d.ts`, so `import { MatButtonAppearance } from '@angular/material/button'` appears in the published typings
  - Instead: a local literal type; check the built `.d.ts`, not just the source
- **Wiring up a mock dependency in a component test "just in case"**
  - Consequence: invents a dependency the component doesn't have
  - Instead: a pure `ds-*` component's test provides only its inputs

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]

# Check list

- [ ] The component's API reads naturally in this application's vocabulary, not Material's
- [ ] No Material type/selector/enum in the component's public API OR the built `types/*.d.ts`
- [ ] The delegate-vs-custom choice is deliberate, documented in the component's goals if it deviates from delegating
- [ ] The component has all four spec layers
- [ ] No component test provides a mock for a dependency the component doesn't inject
- [ ] A form-participating component implements `ControlValueAccessor`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]

# Unittest TestCases

- [ ] WHEN a consumer binds to this component's inputs THEN no knowledge of Angular Material's API is required
- [ ] WHEN Material's own underlying component changes its API in a version bump THEN this component's public API and behaviour are unchanged, absorbing the change internally
- [ ] WHEN the built `types/*.d.ts` is inspected THEN no `@angular/material` import appears
- [ ] WHEN the component's CSS shifts its layout THEN the visual spec fails against its baseline
- [ ] WHEN the component loses an accessible label or drops below the contrast ratio THEN the a11y spec fails

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/Implementation/ComponentLayer/{component-name}.component.ts.create.md|ComponentLayer/{component-name}.component.ts.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]
