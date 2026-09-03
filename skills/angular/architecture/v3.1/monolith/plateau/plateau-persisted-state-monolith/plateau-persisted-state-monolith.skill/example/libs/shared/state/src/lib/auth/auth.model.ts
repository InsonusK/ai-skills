export interface User {
  readonly id: string;
  readonly name: string;
}
export interface LoginCredentials {
  readonly email: string;
  readonly password: string;
}
export interface RefreshResult {
  readonly accessToken: string;
  readonly permissions: string[];
}
