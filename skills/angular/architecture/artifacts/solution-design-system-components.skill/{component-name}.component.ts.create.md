---
description: Generic pattern for authoring any design system component — own selector, signal-based API, independently designed axes, per-component internal implementation decision
project_name: design-system
name: "{component-name}"
artifact_type: component
change_kind: create
---

# How this generic file is used
This is not tied to one concrete component. Any component added to the design system follows this pattern.

# Goals

- Give application developers a component API designed around real usage needs, with zero required knowledge of Angular Material
- Keep the internal implementation free to delegate to Material or be fully custom, per this component's own real requirements

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Component | Ds{ComponentName}Component | DsButtonComponent | ds-{component-name}.component.ts | ds-button.component.ts |
| Selector | ds-{component-name} | ds-button | — | — |

# Implementation changes

Worked example — a button component whose API is organized around this application's real usage axes, not Material's own button categorization, per [[../../../adr/component-encapsulation-strategy.md]]:

```code example
// ds-button.component.ts
export type DsButtonVariant = 'solid' | 'text' | 'outline' | 'fab';
export type DsButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'ds-button',
  template: `
    @if (dropdown() && action()) {
      <!-- split button: action + dropdown together -->
    } @else if (dropdown()) {
      <!-- dropdown only -->
    } @else {
      <!-- plain action button -->
    }
  `,
})
export class DsButtonComponent {
  variant = input<DsButtonVariant>('solid');
  size = input<DsButtonSize>('md');
  color = input<'primary' | 'accent' | 'warn'>('primary');

  // composite fields determining the rendered result — a design decision with
  // no equivalent in Angular Material's own button categorization
  action = input<(() => void) | null>(null);
  dropdown = input<DsDropdownConfig | null>(null);

  clicked = output<void>();
}
```

Internally, `DsButtonComponent` may render Angular Material's `<button mat-button>` (or `mat-fab`, etc.) under the hood for the plain-button case, since Material's own implementation is sufficient there — but none of that is visible in the component's public API.

# Rule changes

## MUST
- The component's inputs/outputs MUST be named and organized around real usage concepts (as in the `variant`/`size`/`color`/`action`/`dropdown` example above), never mirrored from an underlying Material component's own input names.
- If this component wraps a Material component internally, no Material type/enum MUST leak into this component's own input/output types.
- If this component is a form control, it MUST implement `ControlValueAccessor`.

## SHOULD
- The internal implementation SHOULD default to delegating to Angular Material where it fully satisfies the requirement, and SHOULD only be built fully custom when a specific, identified gap justifies it (as in a large-dataset tree needing different performance characteristics than Material's own tree component).

# Anti-patterns

- **Naming an input identically to Material's own corresponding input, with the same enum of values**
  - Consequence: even without directly re-exporting Material's type, this mirrors Material's categorization closely enough that any change to Material's own API will likely force a parallel change here — the encapsulation exists in name only
  - Instead: design the input around this application's own real usage, as the button's `variant`/`action`/`dropdown` example does

# Check list

- [ ] The component's API reads naturally in this application's own vocabulary, not Material's
- [ ] No Material type/selector/enum is exposed through the component's public API
- [ ] The internal implementation choice (delegate to Material vs custom) is a deliberate decision, documented in the component's own goals if it deviates from delegating

# Unittest TestCases

- [ ] WHEN a consumer binds to this component's inputs THEN
  - [ ] no knowledge of Angular Material's own API is required to use it correctly
- [ ] WHEN Angular Material's own underlying component (if used internally) changes its API in a version bump THEN
  - [ ] this component's own public API and behavior remain unchanged, absorbing the change internally
