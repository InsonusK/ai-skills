import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '@org/shared-http-core';
import { LoginCredentials, RefreshResult, User } from './auth.model';

interface LoginResponse {
  user: User;
  accessToken: string;
  permissions: string[];
}

/**
 * Owns the auth HTTP round trips. The `auth` slice's effects call this — they
 * never build a request inline. `silentRefresh` carries no bearer header; it
 * relies solely on the HttpOnly refresh cookie the browser sends automatically.
 */
@Injectable({ providedIn: 'root' })
export class AuthFacade {
  private readonly http = inject(BaseHttpService);

  login(credentials: LoginCredentials): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/auth/login', credentials);
  }

  silentRefresh(): Observable<RefreshResult> {
    return this.http.post<RefreshResult>('/auth/refresh', {});
  }

  logout(): Observable<void> {
    return this.http.post<void>('/auth/logout', {});
  }
}
