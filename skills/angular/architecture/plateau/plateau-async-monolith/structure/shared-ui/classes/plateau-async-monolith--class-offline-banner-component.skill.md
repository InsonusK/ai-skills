---
name: plateau-async-monolith--class-offline-banner-component
description: Shared offline banner component, reading the connectivity slice to inform the user when the application is offline — async-monolith plateau
domain: skill
type: template
plateau: async-monolith
artifact_type: component
version: 20260711190000
tags:
  - skill/template/class
  - plateau/async-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]]"
---

# Goal

- Give the user a clear, persistent signal when the application is offline, backed by the accurate `isOnline` signal from the connectivity slice

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- Reads connectivity state only from `libs/shared/state`'s `connectivity` slice, never `navigator.onLine` directly

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------- | -------------------- | --------- |
| Offline banner | `OfflineBannerComponent` | `OfflineBannerComponent` | `offline-banner.component.ts` | `offline-banner.component.ts` |

# Implementation

```typescript
// Skill: class-offline-banner-component
// Plateau: async-monolith
// Version: 20260711190000

@Component({
  selector: 'app-offline-banner',
  template: `
    @if (!isOnline()) {
      <div role="status" class="offline-banner">You're offline. Showing the latest available data.</div>
    }
  `,
})
export class OfflineBannerComponent {
  protected readonly isOnline = inject(Store).selectSignal(selectIsOnline);
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

# Rules

## MUST
- The banner MUST read `isOnline` from the shared `connectivity` slice — it MUST NOT read `navigator.onLine` directly.
- The banner MUST be mounted once, in `apps/platform-shell`, so it is visible regardless of which feature is currently active.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **A feature implementing its own local offline indicator instead of using this shared component**
  - Consequence: inconsistent messaging and duplicated logic across features
  - Instead: mount `OfflineBannerComponent` once at the shell level; features do not need their own

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

# Check list

- [ ] The banner is mounted exactly once, at the shell level
- [ ] It reads connectivity state only from the shared `connectivity` slice

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]

# Unittest TestCases

- [ ] WHEN `isOnline` is `false` THEN
  - [ ] the banner is visible
- [ ] WHEN `isOnline` is `true` THEN
  - [ ] the banner is not rendered

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-offline-first.skill/solution-offline-first.skill.md|solution-offline-first]] - [[skills/angular/architecture/solutions/solution-offline-first.skill/Implementation/UI/offline-banner.component.ts.create|UI/offline-banner.component.ts.create]]
