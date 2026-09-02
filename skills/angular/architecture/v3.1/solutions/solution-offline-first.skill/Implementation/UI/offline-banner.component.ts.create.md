---
description: Shared offline banner component, reading the connectivity slice to inform the user when the application is offline
project_name: shared-ui
name: offline-banner
element_kind: component
change_kind: create
tags:
  - solution/offline-first
  - element/offline-banner-component-ts
---

# Goals

- Give the user a clear, persistent signal when the application is offline, backed by the accurate `isOnline` signal from the connectivity slice

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------------- | -------------------- | --------- |
| Offline banner | OfflineBannerComponent | OfflineBannerComponent | offline-banner.component.ts | offline-banner.component.ts |

# Implementation changes

```typescript
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

# Rule changes

## MUST
- The banner must read `isOnline` from the shared `connectivity` slice — it must never read `navigator.onLine` directly, to stay consistent with the more accurate combined signal from [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/adr/connectivity-detection.md|connectivity-detection]].
- The banner must be mounted once, in `apps/platform-shell`, so it is visible regardless of which feature or embeddable module is currently active.

## SHOULD
- **A feature implementing its own local offline indicator instead of using this shared component** — Consequence: inconsistent messaging and duplicated logic across features — Instead: mount `OfflineBannerComponent` once at the shell level; features do not need their own

# Check list

- [ ] The banner is mounted exactly once, at the shell level
- [ ] It reads connectivity state only from the shared `connectivity` slice

# Unittest TestCases

- [ ] WHEN `isOnline` is `false` THEN
  - [ ] the banner is visible
- [ ] WHEN `isOnline` is `true` THEN
  - [ ] the banner is not rendered
