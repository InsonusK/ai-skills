import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { HostSession } from './session/host-session';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav>
      <a routerLink="/reports">Reports (remote)</a>
      <button type="button" (click)="toggle()">{{ session.isAuthenticated() ? 'Sign out' : 'Sign in' }}</button>
    </nav>
    <router-outlet />
  `,
})
export class App {
  protected readonly session = inject(HostSession);
  toggle() {
    this.session.isAuthenticated()
      ? this.session.clearSession()
      : this.session.setSession({ id: 'u1', displayName: 'Ada' }, ['reports.view']);
  }
}
