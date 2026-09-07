import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { SESSION_CONTRACT, SessionContract } from '@platform/contracts';
import { requirePermission } from './require-permission';

function fakeSession(over: Partial<{ authed: boolean; perms: string[] }> = {}): SessionContract {
  return {
    currentUser: signal(over.authed ? { id: 'u1', displayName: 'Ada' } : null),
    permissions: signal<readonly string[]>(over.perms ?? []),
    isAuthenticated: signal(over.authed ?? false),
  };
}

function run(session: SessionContract) {
  TestBed.configureTestingModule({ providers: [{ provide: SESSION_CONTRACT, useValue: session }] });
  return TestBed.runInInjectionContext(() =>
    (requirePermission('reports.view') as () => boolean)(),
  );
}

describe('requirePermission (remote guard reading SessionContract)', () => {
  it('denies when the host reports no session', () => {
    expect(run(fakeSession({ authed: false }))).toBe(false);
  });

  it('denies an authenticated user without the permission', () => {
    expect(run(fakeSession({ authed: true, perms: ['reports.export'] }))).toBe(false);
  });

  it('allows an authenticated user with the permission string', () => {
    expect(run(fakeSession({ authed: true, perms: ['reports.view'] }))).toBe(true);
  });
});
