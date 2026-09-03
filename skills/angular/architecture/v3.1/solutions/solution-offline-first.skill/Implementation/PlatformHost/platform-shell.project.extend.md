---
description: Register the Workbox-generated service worker in apps/platform-shell and add the Workbox build step to its project configuration
name: platform-shell
project_kind: application
element_kind: project
change_kind: extend
tags:
  - solution/offline-first
  - element/platform-shell-project
---

# Goals

- Make apps/platform-shell install and use the Workbox-generated service worker produced by the offline-first solution

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    app.config.ts
    main.ts
  project.json
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `src/main.ts` | Extended: registers the generated service worker (`/sw.js`) via `navigator.serviceWorker.register`. |
| `project.json` | Extended: adds a custom Nx build target (or post-build step) invoking `src/sw-build.ts` via `workbox-build`. |

# Implementation changes

```typescript
// apps/platform-shell/src/main.ts
navigator.serviceWorker.register('/sw.js')
  .then(reg => console.log('SW registered', reg))
  .catch(err => console.error('SW registration failed', err));
```

```json
// apps/platform-shell/project.json (excerpt)
{
  "targets": {
    "build-sw": {
      "executor": "nx:run-commands",
      "options": {
        "command": "node apps/platform-shell/src/sw-build.ts"
      }
    }
  }
}
```

# Rules

## MUST
- `main.ts` registers `/sw.js` only after `bootstrapApplication(...)` resolves.
  - Risk: a SW registered before bootstrap races the app for the first requests and can serve a stale shell.
  - Fix: `.then(() => { if (!isDevMode()) navigator.serviceWorker.register('/sw.js'); })` on the bootstrap promise.
- The Workbox build step runs as part of the production build, after the app bundle is written to `dist/apps/platform-shell`.
  - Risk: `injectManifest` run before the bundle exists precaches nothing (or a stale file list).
  - Fix: the `build-sw` target has `dependsOn: [{ target: 'build', params: 'forward' }]`.
- The service worker is never registered in development builds unless explicitly requested.
  - Risk: a dev SW fights live-reload and HMR and serves stale code during development.
  - Fix: guard the `register` call with `!isDevMode()`.
## SHOULD
- **Registering the service worker before the app bootstraps** — Consequence: race conditions where the SW intercepts requests before the app is ready — Instead: register after bootstrap in `main.ts`

# Check list

- [ ] `navigator.serviceWorker.register('/sw.js')` is present in `main.ts`
- [ ] The Workbox build step runs after the production build
- [ ] The service worker is skipped in development builds

# Unittest TestCases

- [ ] WHEN the production build completes THEN
  - [ ] `dist/apps/platform-shell/sw.js` exists
  - [ ] it contains the five routing rules defined in [[skills/angular/architecture/v3.1/solutions/solution-offline-first.skill/Implementation/ServiceWorker/service-worker.create]]
