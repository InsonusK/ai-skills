import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { provideGlobalStore } from './store.config';

describe('provideGlobalStore', () => {
  it('registers an empty-but-wired root NgRx store', () => {
    TestBed.configureTestingModule({ providers: [provideGlobalStore()] });
    const store = TestBed.inject(Store);
    expect(store).toBeTruthy();
    let snapshot: unknown;
    store.subscribe((s) => (snapshot = s)).unsubscribe();
    expect(snapshot).toEqual({});
  });
});
