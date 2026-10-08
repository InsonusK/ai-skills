import { Component, signal } from '@angular/core';
@Component({ selector: 'app-root', template: '<h1>Counter</h1><button (click)="count.set(count() + 1)">Increment</button><output aria-label="Count">{{count()}}</output>' })
export class App { readonly count = signal(0); }
