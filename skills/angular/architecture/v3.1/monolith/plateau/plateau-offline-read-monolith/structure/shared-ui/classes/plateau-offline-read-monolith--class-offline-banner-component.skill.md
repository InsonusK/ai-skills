---
name: plateau-offline-read-monolith--class-offline-banner-component
description: The shared, presentational offline banner in libs/shared/ui — takes an isOnline input, rendered only when offline, mounted once at the shell — offline-read-monolith plateau
domain: skill
type: template
plateau: offline-read-monolith
artifact_type: component
version: 20260903090000
tags:
  - skill/template/class
  - plateau/offline-read-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"

> Lives at `libs/shared/ui/src/lib/offline-banner/`. Presentational: it takes `isOnline` as an input; the shell owns the `Store` and passes `selectIsOnline`. This keeps `libs/shared/ui` free of a `type:store` dependency (the solution's original sketch injected `Store` in the component — this plateau moves that wiring to the shell).

# Goal

- Give the user one clear, persistent signal when the application is offline, driven by the accurate `isOnline` value

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Presentational only — no store, no `navigator.onLine`, no HTTP; it renders based on its `isOnline` input
- `OnPush` change detection; nothing rendered while `isOnline` is `true`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Offline banner | `OfflineBannerComponent` | `OfflineBannerComponent` | `offline-banner.component.ts` | `offline-banner.component.ts` |
| Selector | `ui-{name}` | `ui-offline-banner` | — | — |

# Implementation

```typescript
// Skill: class-offline-banner-component
// Plateau: offline-read-monolith
// Version: 20260903090000

@Component({
  selector: 'ui-offline-banner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (!isOnline()) {
      <div role="status" class="offline-banner">
        You're offline. Showing the latest available data.
      </div>
    }
  `,
})
export class OfflineBannerComponent {
  readonly isOnline = input.required<boolean>();
}
```

```html
<!-- apps/platform-shell/src/app/app.html — mounted once, shell wires the slice -->
<ui-offline-banner [isOnline]="isOnline()" />
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]

# Rules

## MUST
- The component must render offline state from its `isOnline` input only — never inject `Store`, never read `navigator.onLine`.
- It must be mounted exactly once, in `apps/platform-shell`, so it is visible regardless of the active feature.
- The offline message container must carry `role="status"`.
- Never apply several plateau templates per class/artifact.
- Never let a feature build its own local offline indicator — reuse this component.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]

# Check list

- [ ] The banner is mounted exactly once, at the shell level
- [ ] It has no `Store` / `navigator.onLine` reference — only the `isOnline` input
- [ ] Nothing is rendered while `isOnline()` is `true`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]

# Unittest TestCases

- [ ] WHEN `isOnline` is `false` THEN
  - [ ] the `role="status"` banner is visible with the offline message
- [ ] WHEN `isOnline` is `true` THEN
  - [ ] nothing is rendered

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create.md|UI/offline-banner.component.ts.create]]
