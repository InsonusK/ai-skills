import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideMockStore } from '@ngrx/store/testing';
import { App } from './app';
import { appRoutes } from './app.routes';
import { selectIsOnline } from '@org/shared-state';

describe('App shell', () => {
  function setup(isOnline: boolean) {
    return TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter(appRoutes),
        provideMockStore({ selectors: [{ selector: selectIsOnline, value: isOnline }] }),
      ],
    }).compileComponents();
  }

  it('renders the shell with an orders nav link and a router outlet', async () => {
    await setup(true);
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('nav a')?.getAttribute('routerLink')).toBe('/orders');
    expect(el.querySelector('router-outlet')).toBeTruthy();
  });

  it('shows the offline banner when the connectivity slice reports offline', async () => {
    await setup(false);
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('ui-offline-banner [role="status"]')?.textContent).toMatch(/offline/i);
  });
});
