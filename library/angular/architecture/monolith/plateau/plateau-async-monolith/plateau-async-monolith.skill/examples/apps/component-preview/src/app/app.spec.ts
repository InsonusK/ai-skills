import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { appRoutes } from './app.routes';

describe('component-preview app', () => {
  it('boots with a router outlet and no real backend wiring', async () => {
    await TestBed.configureTestingModule({ imports: [App], providers: [provideRouter(appRoutes)] }).compileComponents();
    const f = TestBed.createComponent(App);
    f.detectChanges();
    expect((f.nativeElement as HTMLElement).querySelector('router-outlet')).toBeTruthy();
  });
});
