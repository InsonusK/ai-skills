import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SESSION_CONTRACT } from '@platform/contracts';
import { HasPermissionDirective } from '../session/has-permission.directive';

@Component({
  selector: 'remote-reports',
  imports: [HasPermissionDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (session.isAuthenticated()) {
      <h1>Reports for {{ session.currentUser()?.displayName }}</h1>
      <p>{{ count() }} report(s).</p>
      <button type="button" *hasPermission="'reports.export'">Export</button>
    } @else {
      <p role="status">Sign in on the platform to view reports.</p>
    }
  `,
})
export class ReportsComponent {
  protected readonly session = inject(SESSION_CONTRACT);
  protected readonly count = computed(() => (this.session.isAuthenticated() ? 3 : 0));
}
