import { TestBed } from '@angular/core/testing';
import { RemoteRegistryService, RemoteManifestEntry } from './remote-registry.service';

const MANIFEST: RemoteManifestEntry[] = [
  {
    name: 'embeddable-app',
    remoteEntryUrl: 'http://localhost:4401/remoteEntry.json',
    exposedModule: './Routes',
    routePath: 'reports',
  },
];

describe('RemoteRegistryService', () => {
  let service: RemoteRegistryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RemoteRegistryService);
  });

  describe('load', () => {
    it('populates the remotes signal from the fetched manifest', async () => {
      vi.stubGlobal(
        'fetch',
        vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve(MANIFEST) }),
      );
      await service.load();
      expect(service.remotes()).toEqual(MANIFEST);
    });

    it('rejects (not throws at bootstrap) when the manifest is unreachable', async () => {
      vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));
      await expect(service.load()).rejects.toThrow(/unreachable/i);
    });
  });

  describe('loadRemote', () => {
    it('rejects with a descriptive error for an unknown remote', async () => {
      vi.stubGlobal(
        'fetch',
        vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve(MANIFEST) }),
      );
      await service.load();
      await expect(service.loadRemote('nope')).rejects.toThrow('Unknown remote: nope');
    });
  });
});
