import { Injectable, signal } from '@angular/core';
import { loadRemoteModule } from '@angular-architects/native-federation';

export interface RemoteManifestEntry {
  readonly name: string;
  readonly remoteEntryUrl: string;
  readonly exposedModule: string;
  /** Root segment the shell mounts this remote at. The remote never knows it. */
  readonly routePath: string;
}

/**
 * Resolves which remotes exist and where their `remoteEntry` is served from, at
 * RUNTIME, from an externally updatable manifest — never from the host's build output.
 */
@Injectable({ providedIn: 'root' })
export class RemoteRegistryService {
  private readonly manifest = signal<RemoteManifestEntry[]>([]);
  readonly remotes = this.manifest.asReadonly();

  async load(url = '/remotes-manifest.json'): Promise<void> {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`Remote manifest unreachable: ${response.status}`);
    this.manifest.set((await response.json()) as RemoteManifestEntry[]);
  }

  /** Load a remote's exposed module. A missing/unreachable remote rejects — the caller renders a fallback. */
  async loadRemote(remoteName: string): Promise<unknown> {
    const entry = this.manifest().find((r) => r.name === remoteName);
    if (!entry) throw new Error(`Unknown remote: ${remoteName}`);
    return loadRemoteModule({ remoteEntry: entry.remoteEntryUrl, exposedModule: entry.exposedModule });
  }
}
