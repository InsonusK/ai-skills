import { TestBed } from '@angular/core/testing';
import { SESSION_CONTRACT, SessionContract } from '@platform/contracts';
import { HostSession } from './host-session';

describe('HostSession (SESSION_CONTRACT implementation)', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [{ provide: SESSION_CONTRACT, useExisting: HostSession }],
    });
  });

  it('is the single instance resolved through SESSION_CONTRACT', () => {
    expect(TestBed.inject(SESSION_CONTRACT)).toBe(TestBed.inject(HostSession));
  });

  it('starts unauthenticated', () => {
    const s: SessionContract = TestBed.inject(SESSION_CONTRACT);
    expect(s.isAuthenticated()).toBe(false);
    expect(s.permissions()).toEqual([]);
  });

  it('reflects a login, then a logout, through its signals', () => {
    const host = TestBed.inject(HostSession);
    const contract: SessionContract = TestBed.inject(SESSION_CONTRACT);

    host.setSession({ id: 'u1', displayName: 'Ada' }, ['reports.view']);
    expect(contract.isAuthenticated()).toBe(true);
    expect(contract.currentUser()?.displayName).toBe('Ada');
    expect(contract.permissions()).toContain('reports.view');

    host.clearSession();
    expect(contract.isAuthenticated()).toBe(false);
    expect(contract.permissions()).toEqual([]);
  });
});
