import { signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';
import { SESSION_CONTRACT, SessionContract } from '@platform/contracts';
import { ReportsComponent } from './reports.component';

function fakeSession(over: Partial<{ authed: boolean; perms: string[] }> = {}): SessionContract {
  return {
    currentUser: signal(over.authed ? { id: 'u1', displayName: 'Ada' } : null),
    permissions: signal<readonly string[]>(over.perms ?? []),
    isAuthenticated: signal(over.authed ?? false),
  };
}

describe('ReportsComponent', () => {
  it('renders a not-authenticated state when the host session is anonymous — never its own login', async () => {
    await render(ReportsComponent, {
      providers: [{ provide: SESSION_CONTRACT, useValue: fakeSession({ authed: false }) }],
    });
    expect(screen.getByRole('status')).toHaveTextContent(/sign in on the platform/i);
    expect(screen.queryByRole('button', { name: /export/i })).not.toBeInTheDocument();
  });

  it('shows the report data and gates the Export button on a permission string', async () => {
    await render(ReportsComponent, {
      providers: [
        { provide: SESSION_CONTRACT, useValue: fakeSession({ authed: true, perms: ['reports.export'] }) },
      ],
    });
    expect(screen.getByRole('heading', { name: /reports for ada/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /export/i })).toBeInTheDocument();
  });

  it('hides Export when the permission is absent', async () => {
    await render(ReportsComponent, {
      providers: [{ provide: SESSION_CONTRACT, useValue: fakeSession({ authed: true, perms: [] }) }],
    });
    expect(screen.queryByRole('button', { name: /export/i })).not.toBeInTheDocument();
  });
});
