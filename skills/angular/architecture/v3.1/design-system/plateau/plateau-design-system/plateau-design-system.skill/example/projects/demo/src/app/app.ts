import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav style="display: flex; gap: 16px; padding: 16px; border-bottom: 1px solid var(--mat-sys-outline-variant);">
      <a routerLink="/button">Button</a>
      <a routerLink="/status-chip">Status chip</a>
    </nav>
    <router-outlet />
  `,
})
export class App {}
