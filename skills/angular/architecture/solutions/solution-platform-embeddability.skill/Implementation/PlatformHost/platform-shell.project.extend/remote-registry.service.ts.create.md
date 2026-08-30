---
description: Add a runtime remote registry service that resolves embeddable apps' remoteEntry URLs from configuration instead of build-time federation config
project_name: platform-shell
name: remote-registry
element_kind: service
change_kind: create
tags:
  - solution/platform-embeddability
  - element/remote-registry-service-ts
---

# Goals

- Resolve which embeddable apps are available and where their `remoteEntry` is served from, at runtime, from an externally updatable source (e.g. a config endpoint or a deployed JSON manifest) — not from the host's build output

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ------------ | -------------------- | --------- |
| Runtime remote resolver service | RemoteRegistryService | RemoteRegistryService | remote-registry.service.ts | remote-registry.service.ts |

# Implementation changes

`RemoteRegistryService` fetches a remotes manifest (embeddable app name → remoteEntry URL → exposed module path) and exposes it as a signal so the shell can react to registry updates without a full reload.

```typescript
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

# Rule changes

## MUST
- The manifest fetch MUST happen at runtime (app init or on demand), never be inlined at build time.
- A missing or unreachable remote MUST resolve to an error the caller can catch and render a fallback for — it must not throw uncaught during bootstrap.

# Anti-patterns

- **Caching the manifest for the lifetime of the browser tab with no refresh path**
  - Consequence: a newly deployed embeddable app version, or a newly onboarded app, only becomes visible after a full page reload, undermining the "no platform rebuild needed" benefit of Dynamic Federation
  - Instead: allow re-fetching the manifest (e.g. on shell navigation to a section that hosts remotes, or on an explicit refresh trigger)

# Check list

- [ ] Manifest is fetched at runtime from a location the embeddable app's own deploy pipeline can update independently of the platform
- [ ] A failure to resolve one remote does not prevent other remotes from loading

# Unittest TestCases

- [ ] WHEN the manifest lists a remote THEN
  - [ ] `loadRemoteComponent` resolves and returns its exposed component
- [ ] WHEN the manifest does not list the requested remote THEN
  - [ ] the service rejects with a descriptive error instead of throwing an unhandled exception
