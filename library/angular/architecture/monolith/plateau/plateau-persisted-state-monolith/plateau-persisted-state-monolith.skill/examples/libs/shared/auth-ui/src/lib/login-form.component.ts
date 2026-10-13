import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { AuthActions, selectIsLoggedIn } from '@org/shared-state';

@Component({
  selector: 'auth-login-form',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (isLoggedIn()) {
      <p role="status">You're signed in.</p>
      <button type="button" (click)="logout()">Sign out</button>
    } @else {
      <form (ngSubmit)="submit()">
        <label>Email <input name="email" type="email" [(ngModel)]="email" required /></label>
        <label>Password <input name="password" type="password" [(ngModel)]="password" required /></label>
        <button type="submit">Sign in</button>
      </form>
    }
  `,
})
export class LoginFormComponent {
  private readonly store = inject(Store);
  protected readonly isLoggedIn = this.store.selectSignal(selectIsLoggedIn);
  email = '';
  password = '';

  submit(): void {
    this.store.dispatch(AuthActions.loginRequested({ credentials: { email: this.email, password: this.password } }));
  }
  logout(): void {
    this.store.dispatch(AuthActions.logoutRequested());
  }
}
