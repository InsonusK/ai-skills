---
name: plateau-platform-host--class-remote-registry-service
description: RemoteRegistryService in apps/platform-shell — resolves remotes and their remoteEntry URLs from a RUNTIME manifest, loads an exposed module via loadRemoteModule, and rejects (never throws at bootstrap) for a missing remote — platform-host plateau
domain: skill
type: template
whenToUse: when creating or editing apps/platform-shell/src/app/remote-registry/remote-registry.service.ts, or reviewing how a remote is resolved and mounted
plateau: platform-host
artifact_type: service
version: 20260903180000
tags:
  - skill/template/class
  - plateau/platform-host
  - stack/typescript
  - framework/angular
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]]"
---

> `apps/platform-shell/src/app/remote-registry/remote-registry.service.ts`. `@Injectable({ providedIn: 'root' })`. Loaded once at bootstrap via `provideAppInitializer`.

# Goal

- Resolve which remotes exist and where their `remoteEntry` is served from, at **runtime**, from an externally updatable source — not from the host's build output

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts.create]]

# Naming convention

| use case | class name | file name |
| -------- | ---------- | --------- |
| Runtime remote resolver | `RemoteRegistryService` | `remote-registry.service.ts` |

# Implementation

```typescript
// Skill: class-remote-registry-service
// Plateau: platform-host
import { Injectable, signal } from '@angular/core';
import { loadRemoteModule } from '@angular-architects/native-federation';

export interface RemoteManifestEntry {
  readonly name: string;
  readonly remoteEntryUrl: string;
  readonly exposedModule: string; // e.g. './Routes'
  readonly routePath: string;     // the root segment the shell mounts it at
}

@Injectable({ providedIn: 'root' })
export class RemoteRegistryService {
  private readonly manifest = signal<RemoteManifestEntry[]>([]);
  readonly remotes = this.manifest.asReadonly();

  async load(url = '/remotes-manifest.json'): Promise<void> {
    const res = await fetch(url);
    if (!res.ok) throw new Error(`Remote manifest unreachable: ${res.status}`);
    this.manifest.set(await res.json());
  }

  async loadRemote(remoteName: string): Promise<unknown> {
    const entry = this.manifest().find((r) => r.name === remoteName);
    if (!entry) throw new Error(`Unknown remote: ${remoteName}`);
    return loadRemoteModule({ remoteEntry: entry.remoteEntryUrl, exposedModule: entry.exposedModule });
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts.create]]

# Rules

## MUST
- The manifest fetch happens at runtime (app init or on demand) — never inlined at build time.
- A missing or unreachable remote **rejects** with a catchable error — it must not throw an uncaught exception during bootstrap.
- `loadRemote` returns the exposed module; the caller (a route's `loadChildren`) reads `REMOTE_ROUTES` off it and provides a fallback on rejection.

## SHOULD
- Allow re-fetching the manifest (on navigation to a remote-hosting section, or an explicit refresh) rather than caching it for the whole tab lifetime.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts.create]]

# Check list

- [ ] The manifest is fetched at runtime from a location a remote's own deploy pipeline can update
- [ ] A failure to resolve one remote does not prevent other remotes loading
- [ ] `loadRemote` rejects (not throws) for an unknown remote

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts.create]]

# Unittest TestCases

- [ ] WHEN the manifest lists a remote THEN `loadRemote` resolves and returns its exposed module
- [ ] WHEN the manifest does not list the requested remote THEN the service rejects with `Unknown remote: <name>`
- [ ] WHEN the manifest endpoint returns non-OK THEN `load()` rejects with an `unreachable` error, not an unhandled throw

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/solution-federation-host.skill.md|solution-federation-host]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-host.skill/Implementation/platform-shell.project.extend/remote-registry.service.ts.create.md|remote-registry.service.ts.create]]
