import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { DS_TENANTS, DsTenant } from 'design-system';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav style="display: flex; gap: 16px; padding: 16px; border-bottom: 1px solid var(--mat-sys-outline-variant);">
      <a routerLink="/button">Button</a>
      <a routerLink="/status-chip">Status chip</a>
      <span style="flex: 1"></span>
      <label>
        Tenant:
        <select
          [value]="tenant()"
          (change)="setTenant($any($event.target).value)"
          data-testid="tenant-select"
        >
          <option value="">(base brand)</option>
          @for (t of tenants; track t) {
            <option [value]="t">{{ t }}</option>
          }
        </select>
      </label>
    </nav>
    <router-outlet />
  `,
})
export class App {
  protected readonly tenants = DS_TENANTS;
  protected readonly tenant = signal<DsTenant | ''>(
    (document.documentElement.dataset['tenant'] as DsTenant | undefined) ?? '',
  );

  // Tenant selection is the consuming app's job — the design system only defines
  // the id set and reacts to whatever `data-tenant` is set on <html>.
  protected setTenant(value: DsTenant | ''): void {
    this.tenant.set(value);
    if (value) document.documentElement.dataset['tenant'] = value;
    else delete document.documentElement.dataset['tenant'];
  }
}
