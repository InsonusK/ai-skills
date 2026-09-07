import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'auth-forbidden-page',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section>
      <h1>Not authorized</h1>
      <p>You don't have permission to view this page.</p>
      <a routerLink="/orders">Back to orders</a>
    </section>
  `,
})
export class ForbiddenPageComponent {}
