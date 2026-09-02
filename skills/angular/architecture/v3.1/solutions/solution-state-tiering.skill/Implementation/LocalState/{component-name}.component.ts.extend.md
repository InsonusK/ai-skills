---
description: Generic pattern for component-local state using plain Angular Signals — applies to any component in the application
project_name: "{any}"
name: "{component-name}"
element_kind: component
change_kind: extend
tags:
  - solution/state-tiering
  - element/component-name-component-ts
---

# How this generic file is used
This is not tied to one concrete component. It defines the rule every component in the application follows for state that belongs only to that component (dialog visibility, selected tab, form draft values, component-scoped loading flags).

# Goals

- Keep purely local UI state simple and colocated with the component that owns it, with no store of any kind involved

# Implementation changes

```typescript
@Component({ /* ... */ })
export class ExampleDialogComponent {
  protected readonly isOpen = signal(false);
  protected readonly selectedTabIndex = signal(0);

  open(): void {
    this.isOpen.set(true);
  }
}
```

# Rule changes

## MUST
- State that is read and written only within one component (and optionally its direct children via `input()`) MUST be a plain Signal on that component, not a feature Signal Store or global NgRx state.

- Never introduce a feature Signal Store or a `libs/shared/state` slice purely to hold state no other component or feature ever needs to read.
## SHOULD
- **Creating a feature-level Signal Store for a single dialog's open/closed flag** — Consequence: unnecessary indirection and injection for state nothing outside the component ever reads — Instead: a plain `signal(false)` field on the component
- **Lifting genuinely local state into `libs/shared/state` "just in case it's needed later"** — Consequence: erodes the tiering rule this solution exists to enforce, and clutters global state with noise — Instead: keep it local until a second, unrelated feature/component genuinely needs to read it — then promote it deliberately

# Check list

- [ ] No plain Signal that is only read/written by one component has been promoted to a feature or global store
