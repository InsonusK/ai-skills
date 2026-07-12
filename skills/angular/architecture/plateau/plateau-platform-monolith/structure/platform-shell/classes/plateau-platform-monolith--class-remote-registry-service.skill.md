---
name: plateau-platform-monolith--class-remote-registry-service
description: Runtime service that resolves embeddable apps' remoteEntry URLs from a configuration manifest instead of build-time federation config — platform-monolith plateau
domain: skill
type: template
plateau: platform-monolith
artifact_type: service
version: 20260711210000
tags:
  - skill/template/class
  - plateau/platform-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]]"
---

# Goal

- Resolve which embeddable apps are available and where their `remoteEntry` is served from, at runtime, from an externally updatable source — not from the host's build output

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The manifest is fetched at runtime, never inlined at build time — this is what makes onboarding a new embeddable app a config change, not a platform redeploy

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------ | -------------------- | --------- |
| Runtime remote resolver service | `RemoteRegistryService` | `RemoteRegistryService` | `remote-registry.service.ts` | `remote-registry.service.ts` |

# Implementation

```typescript
// Skill: class-remote-registry-service
// Plateau: platform-monolith
// Version: 20260711150000

@Injectable({ providedIn: 'root' })
export class RemoteRegistryService {
  private readonly manifest = signal<RemoteManifestEntry[]>([]);
  readonly remotes = this.manifest.asReadonly();

  async load(): Promise<void> {
    const response = await fetch('/assets/remotes-manifest.json');
    this.manifest.set(await response.json());
  }

  async loadRemoteComponent(remoteName: string) {
    const entry = this.manifest().find(r => r.name === remoteName);
    if (!entry) throw new Error(`Unknown remote: ${remoteName}`);
    const m = await loadRemoteModule({
      remoteEntry: entry.remoteEntryUrl,
      exposedModule: entry.exposedModule,
    });
    return m.default;
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Rules

## MUST
- The manifest fetch MUST happen at runtime (app init or on demand), never be inlined at build time.
- A missing or unreachable remote MUST resolve to an error the caller can catch and render a fallback for — it must not throw uncaught during bootstrap.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Caching the manifest for the lifetime of the browser tab with no refresh path**
  - Consequence: a newly deployed embeddable app version, or a newly onboarded app, only becomes visible after a full page reload
  - Instead: allow re-fetching the manifest (e.g. on shell navigation to a section hosting remotes, or an explicit refresh trigger)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Check list

- [ ] Manifest is fetched at runtime from a location the embeddable app's own deploy pipeline can update independently of the platform
- [ ] A failure to resolve one remote does not prevent other remotes from loading

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]

# Unittest TestCases

- [ ] WHEN the manifest lists a remote THEN
  - [ ] `loadRemoteComponent` resolves and returns its exposed component
- [ ] WHEN the manifest does not list the requested remote THEN
  - [ ] the service rejects with a descriptive error instead of throwing an unhandled exception

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/solution-platform-embeddability.skill|solution-platform-embeddability]] - [[skills/angular/architecture/solutions/solution-platform-embeddability.skill/Implementation/PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create|PlatformHost/platform-shell.project.extend/remote-registry.service.ts.create]]
