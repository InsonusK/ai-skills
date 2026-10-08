import { Component, signal } from '@angular/core';
import { check, Result } from '../linkcheck/checker';

@Component({ selector: 'app-root', templateUrl: './app.html' })
export class App {
  readonly raw = signal('');
  readonly result = signal<Result | null>(null);

  setUrl(event: Event): void {
    this.raw.set((event.target as HTMLInputElement).value);
  }

  validate(event: Event): void {
    event.preventDefault();
    this.result.set(check(this.raw()));
  }
}
